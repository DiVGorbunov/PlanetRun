using System.Collections;
using UnityEngine;

public class OrbitController : MonoBehaviour
{
    public float X => gameObject.transform.position.x;
    public float Y => gameObject.transform.position.z;
    public float A => gameObject.transform.localScale.x / 2;
    public float B => gameObject.transform.localScale.z / 2;

    public int requiredDestroyedObstacles;

    private GameController gameController;
    private Vector3 portalPosition;
    private bool canPortal;
    public GameObject[] obstacles;
    public float[] obstacleAngles;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public float[] GetRandomAnglesOnPerimeter(int number, float startingAngleInDegrees)
    {
        float[] angles = new float[number];
        float range = 360f / (number + 2);
        float offset = 5f;
        for (int i = 0; i < number; i++)
        {
            var angleInDegrees = Random.Range(startingAngleInDegrees + 3 * range / 2 + i * range + offset,
                startingAngleInDegrees + 3 * range / 2 + (i + 1) * range - offset);
            angleInDegrees = angleInDegrees >= 360 ? angleInDegrees - 360 : angleInDegrees;
            angles[i] = angleInDegrees;
        }

        return angles;
    }

    private Vector3 GetPointByAngle(float angleInDegrees)
    {
        var x = X + (A * Mathf.Cos(angleInDegrees * Mathf.PI / 180));
        var y = Y + (B * Mathf.Sin(angleInDegrees * Mathf.PI / 180));
        var point = new Vector3(x, 0, y);
        return transform.rotation * point;
    }

    public void SpawnNewObstacles(int count, float startingAngle)
    {
        obstacles = new GameObject[count];
        var angles = GetRandomAnglesOnPerimeter(count, startingAngle);
        obstacleAngles = angles;
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetPointByAngle(angles[i]);
            GameObject obstacle = Instantiate(gameController.obstacle, position, Quaternion.identity);
            obstacle.transform.up = transform.up;
            obstacles[i] = obstacle;
        }
    }

    public void RequestSpawnOfObstacles(int index, float startingAngle)
    {
        SpawnNewObstacles(3 * (index + 1), startingAngle);
    }

    public void CreatePortal(Vector3 position)
    {
        portalPosition = position;
        var portal = Instantiate(gameController.portal, position, Quaternion.identity);
        portal.transform.up = transform.up;
        StartCoroutine("CanPortal");
    }

    public IEnumerator CanPortal()
    {
        yield return new WaitForSeconds(0.5f);
        canPortal = true;
    }

    public bool IsAroundPortal(Vector3 position)
    {
        if (canPortal && (portalPosition - position).magnitude < gameController.proximity)
        {
            return true;
        }

        return false;
    }

    private int GetNextObstacleIndex(float currentAngleInDegrees)
    {
        for (int i = 0; i < obstacleAngles.Length - 1; i++)
        {
            if (currentAngleInDegrees >= obstacleAngles[i] && currentAngleInDegrees <= obstacleAngles[i + 1])
            {
                return i + 1;
            }
        }

        return 0;
    }

    public float GetDistanceToNextObstacle(float currentAngleInDegrees)
    {
        Vector3 currentPosition = GetPointByAngle(currentAngleInDegrees);
        int nextObstacleIndex = GetNextObstacleIndex(currentAngleInDegrees);
        return Vector3.Distance(currentPosition, GetPointByAngle(obstacleAngles[nextObstacleIndex]));
    }

    public float GetNextObstacleAngle(float currentAngleInDegrees)
    {
        int nextObstacleIndex = GetNextObstacleIndex(currentAngleInDegrees);
        return obstacleAngles[nextObstacleIndex];
    }

    public void DeactivateNextObstacle(float currentAngleInDegrees)
    {
        var nextObstacle = obstacles[GetNextObstacleIndex(currentAngleInDegrees)];
        nextObstacle.SetActive(false);
        StartCoroutine("RestoreObstacle", nextObstacle);
    }

    private IEnumerator RestoreObstacle(object obstacleObj)
    {
        GameObject obstacle = (GameObject)obstacleObj;
        yield return new WaitForSeconds(2f);
        obstacle.SetActive(true);
    }
}
