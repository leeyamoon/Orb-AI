using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passage : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.gameObject.CompareTag(PLAYER_TAG)) return;
        
        // Load stage
        GameManager.Shared().LoadNextLevel();
        Destroy(gameObject);
    }
}
