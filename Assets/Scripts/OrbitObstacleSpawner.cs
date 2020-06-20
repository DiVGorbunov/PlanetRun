using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitObstacleSpawner : MonoBehaviour
{
    //public OrbitController orbit;
    public GameObject ObstaclePrefab;

    //public int maxCount = 3;

    public static List<GameObject> activeObstacles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SpawnNewObstacles(OrbitController orbit, int count, float startingAngle)
    {
        activeObstacles = new List<GameObject>();
        var positions = orbit.GetRandomPointOnPerimeter(count, startingAngle);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = positions[i];

            GameObject obstacle = Instantiate(ObstaclePrefab, position, Quaternion.identity);
            obstacle.transform.up = orbit.transform.up;
            activeObstacles.Add(obstacle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
