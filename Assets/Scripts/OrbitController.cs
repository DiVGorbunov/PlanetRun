using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class OrbitController : MonoBehaviour
{
    public float X => gameObject.transform.position.x;
    public float Y => gameObject.transform.position.z;
    public float A => gameObject.transform.localScale.x / 2;
    public float B => gameObject.transform.localScale.z / 2;

    public int obstacles;

    public int requiredDestroyedObstacles;

    private GameController gameController;
    private Vector3 portalPosition;
    private bool canPortal;

    // Start is called before the first frame update
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Vector3[] GetRandomPointOnPerimeter(int number, float alpha0)
    {
        Vector3[] points = new Vector3[number];
        float range = 360f / (number + 2);
        float offset = 5f;
        for (int i = 0; i < number; i++)
        {
            var alpha = Random.Range(alpha0 + 3*range/2 + i * range + offset, alpha0 + 3*range / 2 + (i + 1) * range - offset);
            alpha = alpha >= 360 ? alpha - 360 : alpha;
            var x = X + (A * Mathf.Cos(alpha * Mathf.PI / 180));
            var y = Y + (B * Mathf.Sin(alpha * Mathf.PI / 180));
            var point = new Vector3(x, 0, y);
            points[i] = transform.rotation * point;
        }

        return points;
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
}
