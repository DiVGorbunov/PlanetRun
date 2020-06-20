using System.Collections;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public OrbitController[] orbits;
    public SpacecraftController spacecraft;
    public GameObject obstacle;
    public GameObject portal;
    public float proximity = 0.2f;

    private bool isPause;
    public bool IsPause => isPause;

    // Start is called before the first frame update
    void Start()
    {
        isPause = true;
        StartCoroutine("StartGame");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.01f);
        isPause = false;
        spacecraft.SetOrbit(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public OrbitController GetOrbit(int index)
    {
        return orbits[index];
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
