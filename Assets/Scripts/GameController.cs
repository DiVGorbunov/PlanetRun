using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public OrbitController[] orbits;
    public GameObject obstacle;
    public GameObject portal;
    public float proximity = 0.2f;

    public OrbitObstacleSpawner spawner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public OrbitController GetOrbit(int index)
    {
        return orbits[index];
    }

    public void RequestSpawnOfObstacles(int index)
    {
        spawner.SpawnNewObstacles(orbits[index],3*(index+1));
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
