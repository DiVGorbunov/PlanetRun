using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PlayClickSound()
    {
        AudioManager.StaticPlay("click");
    }

    public void PressStart()
    {
        PlayClickSound();
        SceneManager.LoadScene("OrbitScene");
    }

    public void PressHighScores()
    {
        PlayClickSound();
    }
}
