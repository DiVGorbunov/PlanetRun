using System.Collections;
using UnityEngine;

public class OrbitController : MonoBehaviour
{
    public float X => gameObject.transform.position.x;
    public float Y => gameObject.transform.position.z;
    public float A => gameObject.transform.localScale.x / 2;
    public float B => gameObject.transform.localScale.z / 2;

    public int obstaclesCount;
    public (float, float)[] portalIntervals;

    private GameController gameController;
    private Vector3 portalPosition;
    private bool canPortal;
    private GameObject[] obstacles;
    private float[] obstacleAngles;
    private GameObject portal;

    // Start is called before the first frame update
    void Start()
    {
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
        gameController = FindObjectOfType<GameController>();
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

    public void RequestSpawnOfObstacles(float startingAngle)
    {
        SpawnNewObstacles(obstaclesCount, startingAngle);
    }

    public bool IsInPortalInterval(float currentAngleInDegrees)
    {
        for (int i = 0; i < portalIntervals.Length; i++)
        {
            if ((currentAngleInDegrees >= portalIntervals[i].Item1 && currentAngleInDegrees <= portalIntervals[i].Item2) ||
                (portalIntervals[i].Item2 < portalIntervals[i].Item1 && currentAngleInDegrees <= portalIntervals[i].Item2) ||
                (currentAngleInDegrees >= portalIntervals[i].Item1 && portalIntervals[i].Item2 < portalIntervals[i].Item1))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryCreatePortal(float currentAngleInDegrees)
    {
        if (IsInPortalInterval(currentAngleInDegrees))
        {
            portalPosition = GetPointByAngle(currentAngleInDegrees);
            portal = Instantiate(gameController.portal, portalPosition, Quaternion.identity);
            portal.transform.up = transform.up;
            StartCoroutine("CanPortal");

            return true;
        }

        return false;
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
            if ((currentAngleInDegrees >= obstacleAngles[i] && currentAngleInDegrees <= obstacleAngles[i + 1]) ||
                (currentAngleInDegrees >= obstacleAngles[i] && obstacleAngles[i] > obstacleAngles[i + 1]) ||
                (currentAngleInDegrees <= obstacleAngles[i + 1] && obstacleAngles[i] > obstacleAngles[i + 1]))
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

    public float GetDistanceBetweenAngles(float currentAngleInDegrees, float obstacleAngle)
    {
        Vector3 currentPosition = GetPointByAngle(currentAngleInDegrees);
        Vector3 obstaclePosition = GetPointByAngle(obstacleAngle);
        return Vector3.Distance(currentPosition, obstaclePosition);
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
        yield return new WaitForSeconds(gameController.restoreObstacleDelay);
        obstacle.SetActive(true);
    }

    public float GetOrbitSpeed(float spaceCraftSpeed)
    {
        var p = 2 * Mathf.PI * Mathf.Sqrt((A * A + B * B) / 2);
        return spaceCraftSpeed / p;
    }

    void OnDestroy()
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            Destroy(obstacles[i]);
        }
        Destroy(portal);
    }
}
