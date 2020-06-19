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
        gameObject.transform.right = gameObject.transform.position - new Vector3(x, 0, y);
    }
}
