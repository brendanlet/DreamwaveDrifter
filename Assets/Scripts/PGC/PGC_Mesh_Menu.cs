using System;
using System.Collections;
using UnityEngine;

public class PGC_Mesh_Menu : MonoBehaviour
{
    private class Chunk
    {
        public Vector3[] verts;
        public Vector2[] uv;
        public int[] tris;
        public Mesh mesh;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public MeshCollider meshCollider;
        public GameObject gameObject;
        public int x;
        public int z;
    }

    private Chunk[,] chunks;

    [SerializeField]
    [Tooltip("Length / Width of the chunks in meters")]
    private int chunkXSize, chunkZSize;

    [SerializeField]
    [Tooltip("How many layer of perlin noise to apply")]
    private int octaves;

    [SerializeField]
    [Tooltip("Amplitude for perlin noise")]
    private float persistance;

    [SerializeField]
    [Tooltip("Frequency for perlin noise")]
    private float pScale;

    [SerializeField]
    [Tooltip("Scale of height for perlin noise")]
    private float heightScale;

    [SerializeField]
    [Tooltip("Coordinate offset for perlin noise")]
    private float xOffset, zOffset;

    private int xSize = 3;
    private int zSize = 4;

    private void Awake()
    { 
        chunks = new Chunk[xSize, zSize];
        for (int j = 0; j < zSize; j++)
            for (int i = 0; i < xSize; i++)
            {
                chunks[i, j] = new Chunk();
                chunks[i, j].x = i;
                chunks[i, j].z = j;
                chunks[i, j].gameObject = new GameObject("Chunk " + (i + xSize * j));
                chunks[i, j].gameObject.transform.SetParent(transform);

                // We generate the chunk vertices at (0, 0, 0), then move the chunk to its proper position.
                Generate(chunks[i, j]);
                Vector3 chunkPosition = chunks[i, j].gameObject.transform.position;
                chunks[i, j].gameObject.transform.position = new Vector3(chunkPosition.x + chunkXSize * i, chunkPosition.y, chunkPosition.z);
            }
    }

    private void Update()
    {
        for (int j = 0; j < zSize; j++)
        {
            if (chunks[0, j].gameObject.transform.position.x <= 0)
            {
                Destroy(chunks[0, j].gameObject);
                for (int i = 0; i < xSize - 1; i++)
                    chunks[i, j] = chunks[i + 1, j];

                int index = xSize - 1;
                chunks[index, j] = new Chunk();
                chunks[index, j].x = chunks[index - 1, j].x + 1;
                chunks[index, j].z = j;
                chunks[index, j].gameObject = new GameObject("Chunk " + (chunks[index, j].x + xSize * j));
                chunks[index, j].gameObject.transform.SetParent(transform);
                Generate(chunks[index, j]);
                Vector3 chunkPosition = chunks[index - 1, j].gameObject.transform.position;
                chunks[index, j].gameObject.transform.position = new Vector3(chunkPosition.x + chunkXSize, chunkPosition.y, chunkPosition.z);
            }
        }

        foreach (Chunk chunk in chunks)
        {
            Vector3 chunkPosition = chunk.gameObject.transform.position;
            chunk.gameObject.transform.position = new Vector3(chunkPosition.x - 0.1f, chunkPosition.y, chunkPosition.z);
        }
    }

    // Generates a mesh for the given chunk.
    private void Generate(Chunk chunk)
    {
        chunk.meshFilter = chunk.gameObject.AddComponent<MeshFilter>();
        chunk.meshRenderer = chunk.gameObject.AddComponent<MeshRenderer>();
        chunk.meshCollider = chunk.gameObject.AddComponent<MeshCollider>();
        chunk.meshRenderer.material = Resources.Load<Material>("Materials/Grid");

        chunk.verts = new Vector3[(chunkXSize + 1) * (chunkZSize + 1)];
        chunk.uv = new Vector2[chunk.verts.Length];
        chunk.tris = new int[chunkXSize * chunkZSize * 6];

        // Generating vertices, uv, and tangents
        for (int i = 0, z = 0; z <= chunkZSize; z++)
            for (int x = 0; x <= chunkXSize; x++, i++)
            {
                chunk.verts[i] = new Vector3(x, 0, z + chunkZSize * chunk.z);
                chunk.uv[i] = new Vector2(x, z + chunkZSize * chunk.z);
            }

        // Generating triangles
        for (int i = 0, z = 0; z < chunkZSize; z++)
            for (int x = 0; x < chunkXSize; x++, i += 6)
            {
                chunk.tris[i + 5] = 1 + (chunk.tris[i + 4] = chunk.tris[i + 1] = x + (z + 1) * (chunkXSize + 1));
                chunk.tris[i + 3] = chunk.tris[i + 2] = 1 + (chunk.tris[i] = x + z * (chunkXSize + 1));
            }
        PerlinNoise(chunk);
        Flatshading(chunk);

        chunk.mesh = new Mesh();
        chunk.meshFilter.mesh = chunk.mesh;
        chunk.mesh.vertices = chunk.verts;
        chunk.mesh.uv = chunk.uv;
        chunk.mesh.triangles = chunk.tris;
        chunk.mesh.RecalculateNormals();
        chunk.meshCollider.sharedMesh = chunk.mesh;
    }

    // Reconfigures verts, tris, and uv to support flatshading.
    // Makes each triangle have its own unique 3 vertices so that the normals are perpendicular to the triangle face.
    // Flatshading code by Sebastian Lague: https://www.youtube.com/watch?v=V1vL9yRA_eM
    private void Flatshading(Chunk chunk)
    {
        Vector3[] newVerts = new Vector3[chunk.tris.Length];
        Vector2[] newUV = new Vector2[chunk.tris.Length];
        for (int i = 0; i < chunk.tris.Length; i++)
        {
            newVerts[i] = chunk.verts[chunk.tris[i]];
            newUV[i] = chunk.uv[chunk.tris[i]];
            chunk.tris[i] = i;
        }
        chunk.verts = newVerts;
        chunk.uv = newUV;
    }

    // Generates perlin noise in the given chunk.
    private void PerlinNoise(Chunk chunk)
    {
        for (int x = 0; x <= chunkXSize; x++)
            for (int z = 0; z <= chunkZSize; z++)
                chunk.verts[GetVertexIndex(x, z)].y += FBM((x + chunkXSize * chunk.x + xOffset) / chunkXSize, (z + chunkZSize * chunk.z + zOffset) / chunkZSize) * heightScale;
    }

    // Generating multiple perlin noise.
    private float FBM(float x, float z)
    {
        float sum = 0;
        float value = 0;
        float scale = pScale;
        float amplitude = persistance;
        for (int i = 0; i < octaves; i++)
        {
            amplitude /= Mathf.Pow(2, i);
            value += amplitude;
            sum += Mathf.PerlinNoise(x * scale, z * scale) * amplitude;
            scale *= 2;
        }
        sum *= persistance / value;
        return sum;
    }

    // Returns the index for the verts array based on the x and z coords.
    private int GetVertexIndex(int x, int z)
    {
        return (chunkXSize + 1) * z + x;
    }
}