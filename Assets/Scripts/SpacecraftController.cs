using System;
using UnityEngine;
using Input = InputWrapper.Input;

public class SpacecraftController : MonoBehaviour
{
    private float x, y, a, b;
    public float speed = 1;

    private float alpha;
    private int currentIndex;
    private OrbitController currentOrbit;

    private GameController gameController;

    public float coolDownAfterShot = 0.0f;
    public float shotRange = 3.0f;
    public float criticalDistance = 0.1f;
    private float nextObstacleAngle = -1f;
    private bool isNextObstacleDestroyed;
    private float coolDownCounter = 0.5f;

    private int destroyedObstacles = 0;

    public GameObject HUD;

    public ParticleSystem particleSystem;

    private bool needHandlePortalShot = false;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
    }

    public void SetOrbit(int index, float startingAngle)
    {
        currentIndex = index;
        currentOrbit = gameController.GetOrbit(index);
        currentOrbit.RequestSpawnOfObstacles(index, startingAngle);
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
        speed = 1;
        destroyedObstacles = 0;
        nextObstacleAngle = -1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.IsPause)
        {
            return;
        }

        var currentAngleInDegrees = GetCurrentAngleInDegrees();

        if (nextObstacleAngle < 0 || (isNextObstacleDestroyed && currentAngleInDegrees > nextObstacleAngle))
        {
            nextObstacleAngle = currentOrbit.GetNextObstacleAngle(currentAngleInDegrees);
            isNextObstacleDestroyed = false;
        }

        if (coolDownCounter > 0.0f)
        {
            coolDownCounter -= Time.deltaTime;
        }

        /*if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && destroyedObstacles>= currentOrbit.requiredDestroyedObstacles)
            {
                destroyedObstacles -= currentOrbit.requiredDestroyedObstacles;
                particleSystem.maxParticles = destroyedObstacles;
                particleSystem.Stop();
                particleSystem.Play();
                currentOrbit.CreatePortal(gameObject.transform.position);
                speed = 3;
            }
        }*/

        if (needHandlePortalShot)
        {
            needHandlePortalShot = false;

            if (destroyedObstacles >= currentOrbit.requiredDestroyedObstacles)
            {
                destroyedObstacles -= currentOrbit.requiredDestroyedObstacles;
                particleSystem.maxParticles = destroyedObstacles;
                particleSystem.Stop();
                particleSystem.Play();
                currentOrbit.CreatePortal(gameObject.transform.position);
                speed = 3;
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            SetOrbit(currentIndex + 1, GetCurrentAngleInDegrees());
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pressed shot");
            if (coolDownCounter <= 0.0f)
            {
                Debug.Log("Shooting");
                Shoot();
            }
            else
            {
                Debug.Log("Can't shoot");
            }
        }

        if (!isNextObstacleDestroyed && Math.Abs(currentAngleInDegrees - nextObstacleAngle) < criticalDistance)
        {
            Destroy(gameObject);
            HUD.SetActive(true);
        }

        alpha += speed;
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

    private void Shoot()
    {
        //right now just search for closest obstacle
        var currentAngle = GetCurrentAngleInDegrees();
        var closest = currentOrbit.GetDistanceToNextObstacle(currentAngle);
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

    public void RequestPortalShot()
    {
        needHandlePortalShot = true;
    }
}
