using System;
using UnityEngine;
using Input = InputWrapper.Input;
using System.Collections;

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

    private bool isInPortal;
    private bool hasPortal;

    public float spacecraftSpeed = 1500;

    private int destroyedObstacles = 0;
    private bool afterCircle;

    public ParticleSystem particleSystem;
    public GameObject capsule;
    public Material inactiveCapsule, activeCapsule;

    public GameObject RayShotPS;

    private bool needHandlePortalShot = false;
    private bool needHandleLaserShot = false;

    public GameObject SpaceShipModel;
    public ParticleSystem Explosion;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
        RayShotPS.SetActive(false);
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
        hasPortal = false;
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

        if (Input.touchCount > 0 && !needHandlePortalShot)
        {
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                needHandleLaserShot = true;
            }
        }

        if (!hasPortal &&
            destroyedObstacles >= currentOrbit.obstaclesCount &&
            currentOrbit.IsInPortalInterval(currentAngleInDegrees) &&
            !isInPortal)
        {
            ActivateCapsule(true);
            isInPortal = true;
        }

        if (isInPortal &&
            !currentOrbit.IsInPortalInterval(currentAngleInDegrees))
        {
            ActivateCapsule(false);
            isInPortal = false;
        }

        if (needHandlePortalShot)
        {
            needHandlePortalShot = false;

            if (destroyedObstacles >= currentOrbit.obstaclesCount)
            {
                destroyedObstacles -= currentOrbit.obstaclesCount;
                particleSystem.maxParticles = destroyedObstacles;
                particleSystem.Stop();
                particleSystem.Play();
                if (currentOrbit.TryCreatePortal(currentAngleInDegrees))
                {
                    hasPortal = true;
                    ActivateCapsule(false);
                    speed *= gameController.accelerationMultiplier;
                }
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            SetNextOrbit(gameController.GetNextOrbit(), currentAngleInDegrees);
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || needHandleLaserShot)
        {
            needHandleLaserShot = false;
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
            speed = 0;
            this.spacecraftSpeed = 0;
            SpaceShipModel.SetActive(false);
            Explosion.gameObject.SetActive(true);
            gameController.hudController.ShowEndGameScreen();
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

    public delegate void del();

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

        RayShotPS.SetActive(true);
        AudioManager.StaticPlay("laser");
        StartCoroutine(AnimateSize(RayShotPS, 0.0f, 1.7f, 0.05f,new del(()=>{ StartCoroutine(ActivateRay()); })));
    }

    protected IEnumerator AnimateSize(GameObject PS, float startValue, float endValue, float duration, del action)
    {
        float elapsedTime = 0;
        float ratio = elapsedTime / duration;
        while (ratio < 1f)
        {
            elapsedTime += Time.deltaTime;
            ratio = elapsedTime / duration;

            float size = startValue + (endValue - startValue) * ratio;

            setSize(PS, size);

            yield return null;
        }

        if (action != null)
        {
            action();
        }
    }

    private void setSize(GameObject ps, float size)
    {
        Vector3 oldScale = ps.transform.localScale;
        ps.transform.localScale = new Vector3(oldScale.x, oldScale.y, size);
    }

    private IEnumerator ActivateRay()
    {
        yield return new WaitForSeconds(0.1f);
        //RayShotPS.SetActive(false);
        StartCoroutine(AnimateSize(RayShotPS, 1.7f, 0.0f, 0.05f, new del(() => { RayShotPS.SetActive(false); })));
    }

    public void RequestPortalShot()
    {
        needHandlePortalShot = true;
    }

    public void ActivateCapsule(bool isActive)
    {
        var meshRenderer = capsule.GetComponent<MeshRenderer>();
        meshRenderer.material = isActive ? activeCapsule : inactiveCapsule;
    }
}
