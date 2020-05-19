using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{

    public List<GameObject> powerups;
    public int spawnNumber = 25;
    public float powerupHeight = 2.5f;
    private Vector2 dimensions;
    // Start is called before the first frame update
    void Start()
    {
        PGC_Mesh meshMaker = GameObject.Find("PGC_Mesh").GetComponent<PGC_Mesh>();
        dimensions = meshMaker.GetDimensions();
        Populate();
    }

    // Update is called once per frame
    void Populate()
    {
        for(int i = 0; i < spawnNumber; i++)
        {
            Vector3 spawnLocation = new Vector3(Random.Range(0, dimensions.x), 1000f, Random.Range(0, dimensions.y)); //1000f is to start them in the air so we can raycast down
            RaycastHit hit;
            Physics.Raycast(spawnLocation, -Vector3.up, out hit);
            spawnLocation = hit.point;
            spawnLocation.y += powerupHeight;

            GameObject powerup = powerups[(int)Random.Range(0, powerups.Count)];
            Instantiate(powerup,spawnLocation,Quaternion.identity,transform);
        }
    }
}
