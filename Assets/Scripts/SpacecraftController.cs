using System;
using UnityEngine;
using Input = InputWrapper.Input;
using System.Collections;

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
    private float coolDownCounter = 0.0f;
    public float spacecraftSpeed = 1500;

    private int destroyedObstacles = 0;

    public GameObject HUD;

    public ParticleSystem particleSystem;

    public GameObject RayShotPS;

    private bool needHandlePortalShot = false;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
        RayShotPS.SetActive(false);
    }

    public void SetOrbit(int index, float startingAngle)
    {
        currentIndex = index;
        currentOrbit = gameController.GetOrbit(index);
        speed = currentOrbit.GetOrbitSpeed(spacecraftSpeed);
        currentOrbit.RequestSpawnOfObstacles(index, startingAngle);
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
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
                speed *= 3;
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
                speed *= 3;

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

        if (!isNextObstacleDestroyed && currentOrbit.GetDistanceToNextObstacle(currentAngleInDegrees) < criticalDistance)
        {
            Destroy(gameObject);
            HUD.SetActive(true);
        }

        alpha += speed * Time.deltaTime;
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

        RayShotPS.SetActive(true);

        StartCoroutine(AnimateSize(RayShotPS, 0.0f, 1.7f, 0.1f,new del(()=>{ StartCoroutine(ActivateRay()); })));

        //StartCoroutine(ActivateRay());
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
        yield return new WaitForSeconds(0.5f);
        //RayShotPS.SetActive(false);
        StartCoroutine(AnimateSize(RayShotPS, 1.7f, 0.0f, 0.1f, new del(() => { RayShotPS.SetActive(false); })));
    }

    public void RequestPortalShot()
    {
        needHandlePortalShot = true;
    }
}
