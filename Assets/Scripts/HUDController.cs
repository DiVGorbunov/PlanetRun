using UnityEngine;

public class HUDController : MonoBehaviour
{
    public GameObject endGameScreen;

    public void ShowEndGameScreen()
    {
        endGameScreen.SetActive(true);
    }
}
