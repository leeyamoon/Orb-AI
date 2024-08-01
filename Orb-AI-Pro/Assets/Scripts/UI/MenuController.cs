using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Image fadeInImage;
    [SerializeField] private float timeToFade;
    private const float REGULAR_TIME_SCALE = 1f;
    private const float STOP_TIME_SCALE = 0f;
    
    public GameObject menuUI;
    
    private bool isMenuOpen = false;
    
    private void Start()
    {
        CloseMenu();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        Time.timeScale = STOP_TIME_SCALE; // Stop game time
        menuUI.SetActive(true);
        isMenuOpen = true;
        GameManager.Shared().TurnScrollerCursor(false);
    }
    
    public void CloseMenu()
    {
        Time.timeScale = REGULAR_TIME_SCALE; // Resume game time
        menuUI.SetActive(false);
        isMenuOpen = false;
        GameManager.Shared().TurnScrollerCursor(true);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        StartCoroutine(EndingDistance());
    }
    
    private IEnumerator EndingDistance()
    {
        fadeInImage.gameObject.SetActive(true);
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.unscaledDeltaTime * (1 / timeToFade);
            alpha = Math.Min(alpha, 1);
            fadeInImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}