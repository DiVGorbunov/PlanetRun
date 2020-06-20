using System.Collections;
using UnityEngine;

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

    public Vector3 GetRandomPointOnPerimeter()
    {
        var alpha = Random.Range(0, 360);
        var x = X + (A * Mathf.Cos(alpha));
        var y = Y + (B * Mathf.Sin(alpha));
        var point = new Vector3(x, 0, y);
        return transform.rotation * point;
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
