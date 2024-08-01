using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class SpawnPointTrigger : MonoBehaviour
{
    [SerializeField] private int spawnIndex;
    [SerializeField] private Animator animatorToActive;
    
    [SerializeField] private bool isActivateObjects;
    [SerializeField, ConditionalField(nameof(isActivateObjects))] private GameObject gameObjectToActivate;
    [SerializeField, ConditionalField(nameof(isActivateObjects))] private GameObject gameObjectToDeactivate;

    private const string PLAYER_TAG = "Player";
    private const string AUDIO_TAG = "Audio";
    private soundManager _soundManager;

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
    }

    private void Start()
    {
        if(animatorToActive == null)
            return;
        animatorToActive.speed = 0;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        WhenInsideTheCollider(col);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        WhenInsideTheCollider(other);
    }

    private void WhenInsideTheCollider(Collider2D other)
    {
        if (!other.CompareTag(PLAYER_TAG)) return;
        GameManager.Shared().SetCurrentSpawnPoint(spawnIndex);
        _soundManager.PlayEffect(_soundManager.CheckPoint);
        _soundManager.DecreaseLowPassFilter();
        if(animatorToActive != null)
            animatorToActive.speed = 1;
        if (isActivateObjects)
        {
            gameObjectToActivate.SetActive(true);
            gameObjectToDeactivate.SetActive(false);
        }
        Destroy(gameObject);
    }
}
