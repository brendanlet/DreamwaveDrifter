using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public int spawnNumber = 10;
    public float enemyHeight = 2.5f;
    public GameObject enemy;
    private Vector2 dimensions;
    [HideInInspector]
    public static int enemiesRemaining;

    // Start is called before the first frame update
    void Start()
    {
        enemiesRemaining = spawnNumber;
        PGC_Mesh meshMaker = GameObject.Find("PGC_Mesh").GetComponent<PGC_Mesh>();
        dimensions = meshMaker.GetDimensions();
        Populate();
    }

    // Update is called once per frame
    void Populate()
    {
        for (int i = 0; i < spawnNumber; i++)
        {
            Vector3 spawnLocation = new Vector3(Random.Range(0, dimensions.x), 1000f, Random.Range(0, dimensions.y)); //1000f is to start them in the air so we can raycast down
            RaycastHit hit;
            Physics.Raycast(spawnLocation, -Vector3.up, out hit);
            spawnLocation = hit.point;
            spawnLocation.y += enemyHeight;

            Instantiate(enemy, spawnLocation, Quaternion.identity, transform);
        }
    }
}
