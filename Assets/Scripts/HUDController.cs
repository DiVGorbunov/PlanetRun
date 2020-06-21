using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GameObject endGameScreen;
    public GameObject playGameScreen;
    public Text scoreText;

    public void ShowEndGameScreen()
    {
        playGameScreen.SetActive(false);
        endGameScreen.SetActive(true);
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
