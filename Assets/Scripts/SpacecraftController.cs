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

    public float coolDownAfterShot = 0.5f;

    public float shotRange = 3.0f;

    private float coolDownCounter = 0.0f;

    private int destroyedObstacles = 0;

    public GameObject HUD;

    public ParticleSystem particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
        SetOrbit(0);
    }

    void SetOrbit(int index)
    {
        currentIndex = index;
        currentOrbit = gameController.GetOrbit(index);
        gameController.RequestSpawnOfObstacles(index);
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
        speed = 1;
        destroyedObstacles = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (coolDownCounter > 0.0f)
        {
            coolDownCounter -= Time.deltaTime;
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && destroyedObstacles>= currentOrbit.requiredDestroyedObstacles)
            {
                destroyedObstacles -= currentOrbit.requiredDestroyedObstacles;
                particleSystem.maxParticles = destroyedObstacles;
                particleSystem.Stop();
                particleSystem.Play();
                currentOrbit.CreatePortal(gameObject.transform.position);
                speed = 5;
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            SetOrbit(currentIndex + 1);
        }

        alpha += speed;
        gameObject.transform.position = GetSpacecraftPosition(alpha);
        gameObject.transform.LookAt(GetSpacecraftPosition(alpha + 1), currentOrbit.transform.up);

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

        float closest = 100.0f;
        GameObject closestObject = null;
        foreach(var o in OrbitObstacleSpawner.activeObstacles)
        {
            float objDistance = Vector3.Distance(this.gameObject.transform.position, o.transform.position);

            if (objDistance < closest)
            {
                closest = objDistance;
                closestObject = o;
            }
        }
        Debug.Log("Closest is: " + closest);
        if (closest<shotRange)
        {
            OrbitObstacleSpawner.activeObstacles.Remove(closestObject);
            destroyedObstacles += 1;
            particleSystem.maxParticles = destroyedObstacles;
            Destroy(closestObject);
        }

        coolDownCounter = coolDownAfterShot;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Trigger");
        if (collider.gameObject.tag == "Obstacle")
        {
            Debug.Log("Obstacle");
            Destroy(gameObject);

            HUD.SetActive(true);
        }
    }
}
