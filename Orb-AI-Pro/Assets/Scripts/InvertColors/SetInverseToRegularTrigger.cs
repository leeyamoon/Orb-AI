using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInverseToRegularTrigger : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    [SerializeField] private InvertColors invertColors;

    
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PLAYER_TAG) && !invertColors.IsBackgroundWhite())
        {
            invertColors.Invert();
        }
    }
}
