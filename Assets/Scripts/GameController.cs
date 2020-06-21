using System.Collections;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public SpacecraftController spacecraft;
    public GameObject obstacle;
    public GameObject portal;
    public GameObject orbit;
    public float proximity = 0.2f;
    public float restoreObstacleDelay = 2f;
    public float accelerationMultiplier = 2f;

    private OrbitController currentOrbit, nextOrbit;

    void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.01f);

        var orbitGameObject = Instantiate(orbit, Vector3.zero, Quaternion.identity);

        orbitGameObject.transform.localScale = new Vector3(Random.Range(4, 6), 0, Random.Range(4, 6));
        orbitGameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));
        currentOrbit = orbitGameObject.GetComponent<OrbitController>();
        currentOrbit.obstaclesCount = 3;
        currentOrbit.portalIntervals = new[] { (10f, 60f) };
        StartCoroutine(CreateNextOrbit(false));

        spacecraft.StartWithOrbit(currentOrbit);
    }

    public OrbitController GetNextOrbit()
    {
        var result = nextOrbit;
        StartCoroutine(CreateNextOrbit(true));
        return result;
    }

    IEnumerator CreateNextOrbit(bool destroyCurrent)
    {
        yield return new WaitForSeconds(0.01f);

        if (destroyCurrent)
        {
            Destroy(currentOrbit);
            currentOrbit = nextOrbit;
        }

        var orbitGameObject = Instantiate(orbit, Vector3.zero, Quaternion.identity);
        orbitGameObject.transform.localScale = new Vector3(Random.Range(1.5f, 2f) * currentOrbit.transform.localScale.x, 0, Random.Range(1.5f, 2f) * currentOrbit.transform.localScale.z);
        orbitGameObject.transform.rotation = Quaternion.Euler(new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)));

        nextOrbit = orbitGameObject.GetComponent<OrbitController>();
        nextOrbit.obstaclesCount = currentOrbit.obstaclesCount + 3;
        nextOrbit.portalIntervals = new[] { (10f, 60f) };
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
