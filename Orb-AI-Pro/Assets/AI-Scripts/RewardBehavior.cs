using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RewardBehavior : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;
    [SerializeField] private Transform goalTrans;

    private static RewardBehavior _self;
    private float _initDist;

    private void Awake()
    {
        if (_self == null)
            _self = this;
    }

    private void Start()
    {
        _initDist = Vector2.Distance(playerTrans.position, goalTrans.position);
        
    }

    public static RewardBehavior Shared()
    {
        return _self;
    }

    public Vector2 GetGoalPosition()
    {
        return goalTrans.position;
    }

    public float SimpleReward(PlayerState state)
    {
        var dist = Vector2.Distance(playerTrans.position, goalTrans.position);
        var reward =  100 * math.exp(-dist/20);
        //print(reward);
        return reward;
    }
}
