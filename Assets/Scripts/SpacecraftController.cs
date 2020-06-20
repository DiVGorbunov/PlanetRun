using System;
using UnityEngine;
using Input = InputWrapper.Input;

public class SpacecraftController : MonoBehaviour
{
    private float x, y, a, b;
    public float speed = 1;

    private float alpha;
    private OrbitController currentOrbit;

    private GameController gameController;

    public float coolDownAfterShot = 0.0f;
    public float shotRange = 3.0f;
    public float criticalDistance = 0.1f;
    private float nextObstacleAngle = -1f;
    private bool isNextObstacleDestroyed;
    private float coolDownCounter = 0.0f;
    public float spacecraftSpeed = 1500;

    private int destroyedObstacles = 0;
    private bool afterCircle;

    public GameObject HUD;

    public ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
    }

    void SetNextOrbit(OrbitController nextOrbit, float startingAngle)
    {
        currentOrbit = nextOrbit;
        speed = currentOrbit.GetOrbitSpeed(spacecraftSpeed);
        currentOrbit.RequestSpawnOfObstacles(startingAngle);
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
        destroyedObstacles = 0;
        nextObstacleAngle = -1f;
        afterCircle = false;
    }

    public void StartWithOrbit(OrbitController orbit)
    {
        SetNextOrbit(orbit, 0);
    } 

    // Update is called once per frame
    void Update()
    {
        if (currentOrbit == null)
        {
            return;
        }

        var currentAngleInDegrees = GetCurrentAngleInDegrees();

        if (nextObstacleAngle < 0 || (isNextObstacleDestroyed && currentAngleInDegrees > nextObstacleAngle && !afterCircle))
        {
            nextObstacleAngle = currentOrbit.GetNextObstacleAngle(currentAngleInDegrees);
            if (nextObstacleAngle < currentAngleInDegrees)
            {
                afterCircle = true;
            }
            isNextObstacleDestroyed = false;
        }

        if (coolDownCounter > 0.0f)
        {
            coolDownCounter -= Time.deltaTime;
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && destroyedObstacles>= currentOrbit.obstaclesCount)
            {
                destroyedObstacles -= currentOrbit.obstaclesCount;
                particleSystem.maxParticles = destroyedObstacles;
                particleSystem.Stop();
                particleSystem.Play();
                currentOrbit.CreatePortal(gameObject.transform.position);
                speed *= 3;
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            SetNextOrbit(gameController.GetNextOrbit(), currentAngleInDegrees);
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pressed shot");
            if (coolDownCounter <= 0.0f)
            {
                Debug.Log("Shooting");
                Shoot(currentAngleInDegrees);
            }
            else
            {
                Debug.Log("Can't shoot");
            }
        }

        var distance = currentOrbit.GetDistanceBetweenAngles(currentAngleInDegrees, nextObstacleAngle);

        Debug.Log("Angle: " + currentAngleInDegrees + " Next Obstacle Angle: " + nextObstacleAngle + " Is Destroyed: " + isNextObstacleDestroyed + " Distance: " + distance);

        if (!isNextObstacleDestroyed && distance < criticalDistance)
        {
            Destroy(gameObject);
            HUD.SetActive(true);
        }

        alpha += speed * Time.deltaTime;
        if (GetCurrentAngleInDegrees() < currentAngleInDegrees)
        {
            afterCircle = false;
        }
        gameObject.transform.position = GetSpacecraftPosition(alpha);
        gameObject.transform.LookAt(GetSpacecraftPosition(alpha + 1), currentOrbit.transform.up);
    }

    private float GetCurrentAngleInDegrees()
    {
        var originalAngle = alpha * 0.005f * 180 / (float)Math.PI;
        while (originalAngle > 360)
        {
            originalAngle -= 360;
        }
        
        return originalAngle;
    }

    private Vector3 GetSpacecraftPosition(float newAlpha)
    {
        var X = x + (a * Mathf.Cos(newAlpha * .005f));
        var Y = y + (b * Mathf.Sin(newAlpha * .005f));
        var rotation = currentOrbit.transform.rotation;
        return rotation * new Vector3(X, 0, Y);
    }

    private void Shoot(float currentAngle)
    {
        var closest = currentOrbit.GetDistanceBetweenAngles(currentAngle, nextObstacleAngle);
        Debug.Log("Closest is: " + closest);
        if (closest<shotRange)
        {
            destroyedObstacles += 1;
            currentOrbit.DeactivateNextObstacle(currentAngle);
            isNextObstacleDestroyed = true;
            particleSystem.maxParticles = destroyedObstacles;
        }

        coolDownCounter = coolDownAfterShot;
    }
}
