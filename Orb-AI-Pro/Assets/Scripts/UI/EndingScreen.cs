using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScreen : MonoBehaviour
{
    
    private void OnEnable()
    {
        Time.timeScale = 0;
        GameManager.Shared().TurnScrollerCursor(false);
    }

    private void OnDisable()
    {
        GameManager.Shared().TurnScrollerCursor(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
}
