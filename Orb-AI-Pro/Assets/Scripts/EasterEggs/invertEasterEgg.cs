using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class invertEasterEgg : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";
    [SerializeField] private InvertColors invertColors;
    [SerializeField] private bool createObjOnUse;

    [SerializeField, ConditionalField(nameof(createObjOnUse))]
    private GameObject gameObjectToCreate;

    [SerializeField, ConditionalField(nameof(createObjOnUse))]
    private Vector2 newObjPos;
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(PLAYER_TAG))
        {
            invertColors.Invert();
            if (createObjOnUse)
            {
                Instantiate(gameObjectToCreate, newObjPos, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
    
}
