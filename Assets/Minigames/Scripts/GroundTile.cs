using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject coinPrefab;

    void Start()
    {
        groundSpawner = GameObject.FindObjectOfType<GroundSpawner>();

    }


    private void OnTriggerExit (Collider other)
    {
        // Spawn tile
        groundSpawner.SpawnTile(true);
        Destroy(gameObject, 2);
    }

    public void SpawnObstacle()
    {
        //choose a random point to spawn obstacle
        int obstacleSpawnIndex = Random.Range(2, 5);
        Transform spawnPoint = transform.GetChild(obstacleSpawnIndex).transform;

        // Spawn the obstacle at the position
        Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity, transform);


    }

   
    public void SpawnCoins() 
    {
        int coinsToSpawn = 10;
        for (int i = 0; i < coinsToSpawn; i++)
        {
            GameObject temp = Instantiate(coinPrefab, transform);
            temp.transform.position = GetRandomPointInCollider(GetComponent<Collider>());
        }
    } 

    Vector3 GetRandomPointInCollider (Collider collider)
    {
        Vector3 point = new Vector3(
                         Random.Range(collider.bounds.min.x, collider.bounds.max.x),
                         Random.Range(collider.bounds.min.y, collider.bounds.max.y),
                         Random.Range(collider.bounds.min.z, collider.bounds.max.z)
                        );
        if (point != collider.ClosestPoint(point))
        {
            point = GetRandomPointInCollider(collider);
        }
        point.y = 1;
        return point;
    }



}
