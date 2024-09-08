using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingScreen : MonoBehaviour
{
    public void OpenQLearn()
    {
        SceneManager.LoadScene("Scenes/Game-QLearn");
    }
    
    public void OpenGame()
    {
        SceneManager.LoadScene("Game-Play");
    }
    
    public void OpenHeuristic()
    {
        SceneManager.LoadScene("Game-Heuristic");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
