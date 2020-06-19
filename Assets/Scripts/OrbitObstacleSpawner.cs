using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitObstacleSpawner : MonoBehaviour
{
    public OrbitController orbit;
    public GameObject ObstaclePrefab;

    public int maxCount = 3;

    public static List<GameObject> activeObstacles;
    // Start is called before the first frame update
    void Start()
    {
        activeObstacles = new List<GameObject>();
        for (int i = 0; i < maxCount; i++)
        {
            Vector3 position = orbit.GetRandomPointOnPerimeter();
           
            GameObject obstacle = Instantiate(ObstaclePrefab, position, Quaternion.identity);
            activeObstacles.Add(obstacle);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
