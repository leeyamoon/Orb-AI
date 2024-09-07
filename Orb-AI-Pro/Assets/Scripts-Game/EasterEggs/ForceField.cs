using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField] private MovementParent.XMovement xMove;
    [SerializeField] private MovementParent.YMovement yMove;

    private PlayerAction _action;

    private void Start()
    {
        _action = new PlayerAction(xMove, yMove);
    }

    public PlayerAction GetForceAction()
    {
        return _action;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(!col.CompareTag("Player"))
            return;
        MovementParent mp = col.GetComponent<MovementParent>();
        if(mp == null)
            return;
        mp.curPolicy = this;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.CompareTag("Player"))
            return;
        MovementParent mp = other.GetComponent<MovementParent>();
        if(mp == null)
            return;
        mp.curPolicy = null;
    }
    
}
