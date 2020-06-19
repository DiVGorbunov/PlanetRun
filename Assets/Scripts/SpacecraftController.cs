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
    }
}
