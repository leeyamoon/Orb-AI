using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeWhenTouch : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    
    [SerializeField] private Image fadeInImage;
    [SerializeField] private GameObject endingMenu;

    [SerializeField, Min(0.01f)] private float timeToFade;
    
    private bool _triggerEnding = false;
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_triggerEnding && col.CompareTag(PLAYER_TAG))
        {
            _triggerEnding = true;
            StartCoroutine(EndingDistance());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_triggerEnding && other.CompareTag(PLAYER_TAG))
        {
            StartCoroutine(EndingDistance());
        }
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
        yield return new WaitForSeconds(timeToFade +0.5f);
        endingMenu.SetActive(true);
    }
}
