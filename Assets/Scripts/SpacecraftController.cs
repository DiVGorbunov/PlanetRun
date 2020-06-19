using UnityEngine;

public class SpacecraftController : MonoBehaviour
{
    private float x, y, a, b;
    public OrbitController currentOrbit;

    private float alpha;

    // Start is called before the first frame update
    void Start()
    {
        x = currentOrbit.X;
        y = currentOrbit.Y;
        a = currentOrbit.A;
        b = currentOrbit.B;
    }

    // Update is called once per frame
    void Update()
    {
        alpha += 1;
        var X = x + (a * Mathf.Cos(alpha * .005f));
        var Y = y + (b * Mathf.Sin(alpha * .005f));
        gameObject.transform.position = new Vector3(X, 0, Y);
        gameObject.transform.right = gameObject.transform.position - new Vector3(x, 0, y);

        if (Input.GetKeyDown(KeyCode.Space))
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
