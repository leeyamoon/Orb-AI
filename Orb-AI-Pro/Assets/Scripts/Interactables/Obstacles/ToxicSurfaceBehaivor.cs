using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicSurfaceBehaivor : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(PLAYER_TAG))
        {
            GameManager.Shared().RespawnPlayer(true);
        }
    }
}
