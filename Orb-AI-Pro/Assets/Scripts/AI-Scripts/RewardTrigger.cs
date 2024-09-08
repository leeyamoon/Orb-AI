using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class RewardTrigger : MonoBehaviour
{
    [SerializeField] private int spawnIndex;

    [SerializeField] private bool isActivateObjects;
    [SerializeField, ConditionalField(nameof(isActivateObjects))] private GameObject gameObjectToActivate;
    [SerializeField, ConditionalField(nameof(isActivateObjects))] private GameObject gameObjectToDeactivate;
    private bool _visited;

    private void Start()
    {
        _visited = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(_visited)
            return;
        if (!col.CompareTag("Player")) return;
        _visited = true;
        RewardBehavior.Shared().IndexUp();
        WhenInsideTheCollider();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(_visited)
            return;
        if (!other.CompareTag("Player")) return;
        _visited = true;
        RewardBehavior.Shared().IndexUp();
        WhenInsideTheCollider();
    }

    private void WhenInsideTheCollider()
    {
        GameManager.Shared().SetCurrentSpawnPoint(spawnIndex);
        GameManager.Shared().PrintAllCp();
        if (isActivateObjects)
        {
            gameObjectToActivate.SetActive(true);
            gameObjectToDeactivate.SetActive(false);
        }
    }
}
