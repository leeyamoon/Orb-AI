using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardTrigger : MonoBehaviour
{
    private bool _visited;

    private void Start()
    {
        _visited = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(_visited)
            return;
        _visited = true;
        RewardBehavior.Shared().IndexUp();
    }
}
