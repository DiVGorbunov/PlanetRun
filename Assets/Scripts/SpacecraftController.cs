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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                currentOrbit.CreatePortal(gameObject.transform.position);
                speed = 5;
            }
        }

        if (currentOrbit.IsAroundPortal(gameObject.transform.position))
        {
            SetOrbit(currentIndex + 1);
        }

        alpha += speed;
        var X = x + (a * Mathf.Cos(alpha * .005f));
        var Y = y + (b * Mathf.Sin(alpha * .005f));
        gameObject.transform.position = new Vector3(X, 0, Y);
        gameObject.transform.right = gameObject.transform.position - new Vector3(x, 0, y);

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Pressed shot");
            Shoot();
        }
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
        if (closest<3.0f)
        {
            OrbitObstacleSpawner.activeObstacles.Remove(closestObject);
            Destroy(closestObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Trigger");
        if (collider.gameObject.tag == "Obstacle")
        {
            Debug.Log("Obstacle");
            Destroy(gameObject);
        }
    }
}
