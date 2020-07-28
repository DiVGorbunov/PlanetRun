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
    public Material inactiveCapsule, activeCapsule, lightupCapsule;
    bool isCapsuleActivated = false;

    public GameObject RayShotPS;

    private bool needHandlePortalShot = false;
    private bool needHandleLaserShot = false;

    private int currentScore = 0;
    private float orbitAlpha = 0;
    private int currentLevel = 0;

    public GameObject SpaceShipModel;
    public ParticleSystem Explosion;

    private bool isDead = false;

    private Camera camera;
    Vector3 cameraPos;
    float cameraInterpolationTime = 0.2f;

    public GameObject tutorialSuck;
    public GameObject tutorialPortal;
    public GameObject tutorialShoot;

    bool needPortalTutorial = true;
    public bool LastTutorialShown = false;

    public ParticleSystem SpeedPS;
    public GameObject ShipTrail;

    public Material skyMat;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
        RayShotPS.SetActive(false);
        camera = GetComponentInChildren<Camera>();
        cameraPos = camera.transform.localPosition;

        PlayerData data = SaveSystem.Load();
        if (!data.isNew)
        {
            needPortalTutorial = false;
            timeForTutorial = -1.0f;
        }
        else
        {
            needPortalTutorial = true;
            timeForTutorial = 1.5f;
        }
    }

    void SetNextOrbit(OrbitController nextOrbit, float startingAngle)
    {
        currentOrbit = nextOrbit;
        speed = currentOrbit.GetOrbitSpeed(spacecraftSpeed);
        StartCoroutine(AnimateCameraPos(camera.transform.localPosition, cameraPos, 70.0f, 60.0f, cameraInterpolationTime));
        SpeedPS.gameObject.SetActive(false);
        ShipTrail.SetActive(false);
        speedMode = false;
        currentOrbit.RequestSpawnOfObstacles(startingAngle);
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
        destroyedObstacles = 0;
        nextObstacleAngle = -1f;
        afterCircle = false;
        hasPortal = false;
        orbitAlpha = alpha;
        currentLevel++;
    }

    public void StartWithOrbit(OrbitController orbit)
    {
        SetNextOrbit(orbit, 0);
    }

    float timeForTutorial = 1.5f;

    // Update is called once per frame
    void Update()
    {
        if (timeForTutorial > 0.0f)
        {
            timeForTutorial -= Time.deltaTime;
            if (timeForTutorial <= 0.0f)
            {
                Time.timeScale = 0.0f;
                if (needPortalTutorial)
                {
                    tutorialSuck.SetActive(true);
                }
                else
                {
                    tutorialShoot.SetActive(true);
                    LastTutorialShown = true;
                }
            }
        }


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

        if (Input.touchCount > 0 && !needHandlePortalShot && !isDead)
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
            //ActivateCapsule(false);
            isInPortal = false;
        }

        if (needHandlePortalShot)
        {
            needHandlePortalShot = false;

            if (destroyedObstacles >= currentOrbit.obstaclesCount)
            {
                if (currentOrbit.TryCreatePortal(currentAngleInDegrees))
                {
                    destroyedObstacles -= currentOrbit.obstaclesCount;
                    particleSystem.Clear();
                    hasPortal = true;
                    ActivateCapsule(false);
                    var meshRenderer = capsule.GetComponent<MeshRenderer>();
                    meshRenderer.material = lightupCapsule;
                    speed *= gameController.accelerationMultiplier;
                    StartCoroutine(AnimateCameraPos(cameraPos, cameraPos - new Vector3(0.0f, 0.0f, 0.5f), 60.0f, 70.0f, cameraInterpolationTime));
                    speedMode = true;
                    SpeedPS.gameObject.SetActive(true);
                    ShipTrail.SetActive(true);
                }
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            currentScore += GetScore(true, false, GetLap(), speed, currentLevel);
            gameController.hudController.SetScore(currentScore);
            AudioManager.StaticPlay("enter-portal");
            SetNextOrbit(gameController.GetNextOrbit(), currentAngleInDegrees);

            camera.GetComponent<Skybox>().material = skyMat;
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

        if (!isNextObstacleDestroyed && distance < criticalDistance && !isDead)
        {
            speed = 0;
            this.spacecraftSpeed = 0;
            SpaceShipModel.SetActive(false);
            Explosion.gameObject.SetActive(true);
            gameController.hudController.ShowEndGameScreen();
            AudioManager.StaticPlay("died");
            isDead = true;

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

    Vector3 m_WeaponBobLocalPosition;

    [Header("Weapon Bob")]
    [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
    public float bobFrequency = 3f;
    [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
    public float bobSharpness = 10f;
    [Tooltip("Distance the weapon bobs when not aiming")]
    public float defaultBobAmount = 0.05f;
    [Tooltip("Distance the weapon bobs when aiming")]
    public float aimingBobAmount = 0.02f;

    float m_WeaponBobFactor = 0.5f;

    bool speedMode = false;

    private void LateUpdate()
    {
        if (speedMode)
        {
            float bobAmount = defaultBobAmount;
            float frequency = bobFrequency;
            float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * m_WeaponBobFactor;
            float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.3f) * bobAmount * m_WeaponBobFactor;

            // Apply weapon bob
            m_WeaponBobLocalPosition.x = hBobValue;
            m_WeaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            SpaceShipModel.transform.localPosition = m_WeaponBobLocalPosition;
        }        
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
            var isPlanet = currentOrbit.DeactivateNextObstacle(currentAngle);
            isNextObstacleDestroyed = true;
            if (isPlanet)
            {
                particleSystem.Emit(1);
            }            

            currentScore += GetScore(false, isPlanet, GetLap(), speed, currentLevel);
            gameController.hudController.SetScore(currentScore);

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

    protected IEnumerator AnimateCameraPos(Vector3 startPos, Vector3 endPos, float startFov, float endFov, float duration)
    {
        float elapsedTime = 0;
        float ratio = elapsedTime / duration;
        while (ratio < 1f)
        {
            elapsedTime += Time.deltaTime;
            ratio = elapsedTime / duration;

            Vector3 pos = startPos + (endPos - startPos) * ratio;

            //camera.transform.localPosition = pos;
            camera.fieldOfView = startFov + (endFov - startFov) * ratio;
            SpeedPS.gameObject.GetComponent<ParticleSystemRenderer>().lengthScale = 0 + (35 - 0) * ratio;

            yield return null;
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
        if (Time.timeScale == 0.0f)
        {
            timeForTutorial = 0.5f;
            HideTutorials();
        }

        if (isDead)
        { return; }
        if (!isCapsuleActivated)
        { return; }
        needHandlePortalShot = true;
    }

    public void ActivateCapsule(bool isActive)
    {
        var meshRenderer = capsule.GetComponent<MeshRenderer>();
        meshRenderer.material = isActive ? activeCapsule : inactiveCapsule;
        isCapsuleActivated = isActive;
        if (needPortalTutorial && isActive)
        {
            needPortalTutorial = false;
            tutorialPortal.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    private int GetLap()
    {
        var originalAngle = (int) ((alpha - orbitAlpha) * 0.005f * 180 / (float)Math.PI);
        return (originalAngle / 360) + 1;
    }

    public int GetScore(bool isPortal, bool isPlanet, int lap, float speed, int level)
    {
        if (isPortal)
        {
            return 10000 * level / lap;
        }

        if (isPlanet)
        {
            return (int) (6 * speed);
        }

        return (int) (20 * level * speed / lap);
    }

    public void Shoot()
    {
        needHandleLaserShot = true;
        HideTutorials();
    }

    public void HideTutorials()
    {
        Time.timeScale = 1.0f;
        tutorialSuck.SetActive(false);
        tutorialPortal.SetActive(false);
        tutorialShoot.SetActive(false);
    }
}
