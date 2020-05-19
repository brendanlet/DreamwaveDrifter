using UnityEngine;

public class PGC_Mesh : MonoBehaviour
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
    [Tooltip("Length / Width of the mesh in chunks")]
    private int xSize, zSize;

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

    [SerializeField]
    [Tooltip("Height of main mountain in meters")]
    private float mountainHeight;

    [SerializeField]
    [Tooltip("Slope of main mountain (should be positive)")]
    private float mountainSlope;

    private bool done = false;


    private void Awake()
    {
        // Generating an xSize by zSize grid of chunks, with each chunk being a chunkXSize by chunkZSize grid.
        chunks = new Chunk[xSize, zSize];
        for (int j = 0; j < zSize; j++)
            for (int i = 0; i < xSize; i++)
            {
                chunks[i, j] = new Chunk();
                chunks[i, j].x = i;
                chunks[i, j].z = j;
                chunks[i, j].gameObject = new GameObject("Chunk " + (i + xSize * j));
                chunks[i, j].gameObject.transform.SetParent(transform);
                chunks[i, j].meshFilter = chunks[i, j].gameObject.AddComponent<MeshFilter>();
                chunks[i, j].meshRenderer = chunks[i, j].gameObject.AddComponent<MeshRenderer>();
                chunks[i, j].meshCollider = chunks[i, j].gameObject.AddComponent<MeshCollider>();
                chunks[i, j].meshRenderer.material = Resources.Load<Material>("Materials/Grid");
                Generate(chunks[i, j]);

                chunks[i, j].gameObject.layer = LayerMask.NameToLayer("Terrain");
            }
    }

    // Generates a mesh for the given chunk.
    private void Generate(Chunk chunk)
    {
        chunk.verts = new Vector3[(chunkXSize + 1) * (chunkZSize + 1)];
        chunk.uv = new Vector2[chunk.verts.Length];
        chunk.tris = new int[chunkXSize * chunkZSize * 6];

        // Generating vertices, uv, and tangents
        for (int i = 0, z = 0; z <= chunkZSize; z++)
            for (int x = 0; x <= chunkXSize; x++, i++)
            {
                chunk.verts[i] = new Vector3(x + chunkXSize * chunk.x, 0, z + chunkZSize * chunk.z);
                chunk.uv[i] = new Vector2(x + chunkXSize * chunk.x, z + chunkZSize * chunk.z);
            }

        // Generating triangles
        for (int i = 0, z = 0; z < chunkZSize; z++)
            for (int x = 0; x < chunkXSize; x++, i += 6)
            {
                chunk.tris[i + 5] = 1 + (chunk.tris[i + 4] = chunk.tris[i + 1] = x + (z + 1) * (chunkXSize + 1));
                chunk.tris[i + 3] = chunk.tris[i + 2] = 1 + (chunk.tris[i] = x + z * (chunkXSize + 1));
            }
        MountainCurved(chunk, xSize * chunkXSize / 2, zSize * chunkZSize / 2, mountainHeight, mountainSlope);
        PerlinNoise(chunk);
        MountainLinear(chunk, xSize * chunkXSize / 2, zSize * chunkZSize / 2, 2f, 0.05f);
        Flatshading(chunk);
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
                chunk.verts[GetVertexIndex(x, z)].y += FBM((x + chunkXSize * chunk.x + xOffset) / xSize / chunkXSize, (z + chunkZSize * chunk.z + zOffset) / zSize / chunkZSize) * heightScale;
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

    // Generates a piece of a mountain in the given chunk at the global point (moutainX, mountainZ) with given height and slope.
    private void MountainLinear(Chunk chunk, int mountainX, int mountainZ, float height, float slope)
    {
        for (int x = 0; x <= chunkXSize; x++)
            for (int z = 0; z <= chunkZSize; z++)
            {
                float distance = new Vector2(x + chunkXSize * chunk.x - mountainX, z + chunkZSize * chunk.z - mountainZ).magnitude;
                int vertex = GetVertexIndex(x, z);
                chunk.verts[vertex].y += height - slope * distance;
                if (chunk.verts[vertex].y < 0)
                    chunk.verts[vertex].y = 0;
            }
    }

    private void MountainCurved(Chunk chunk, int mountainX, int mountainZ, float height, float slope)
    {
        for (int x = 0; x <= chunkXSize; x++)
            for (int z = 0; z <= chunkZSize; z++)
            {
                float distance = new Vector2(x + chunkXSize * chunk.x - mountainX, z + chunkZSize * chunk.z - mountainZ).magnitude;
                int vertex = GetVertexIndex(x, z);
                float localSlope = slope / (1 + distance * 0.04f);
                chunk.verts[vertex].y += height - localSlope * distance;
                if (chunk.verts[vertex].y < 0)
                    chunk.verts[vertex].y = 0;
            }
    }

    // Returns the index for the verts array based on the x and z coords.
    private int GetVertexIndex(int x, int z)
    {
        return (chunkXSize + 1) * z + x;
    }

    private void Start()
    {
        // Applying generated meshes to each chunk.
        foreach (Chunk chunk in chunks)
        {
            chunk.mesh = new Mesh();
            chunk.meshFilter.mesh = chunk.mesh;
            chunk.mesh.vertices = chunk.verts;
            chunk.mesh.uv = chunk.uv;
            chunk.mesh.triangles = chunk.tris;
            chunk.mesh.RecalculateNormals();
            chunk.meshCollider.sharedMesh = chunk.mesh;
        }
        done = true;
    }

    public Vector2 GetDimensions()
    {
        Vector2 dimensions = new Vector2();
        dimensions.x = chunkXSize * xSize;
        dimensions.y = chunkZSize * zSize;

        return dimensions;

    }

    public bool isDone()
    {
        return done;
    }
}