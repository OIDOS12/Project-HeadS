using Mirror;
using UnityEngine;

public class ServerSpawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Assign your prefab in the Inspector
    public bool isSpawned = false;
    public void Update()
    {
        // Check if we are running as a server.
        if (NetworkServer.active && isSpawned == false)
        {
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        // Instantiate the object on the server.

            GameObject spawnedObject = Instantiate(objectToSpawn);

        // Spawn the object on the network.
            NetworkServer.Spawn(spawnedObject);
            isSpawned = true;
        
    }
}