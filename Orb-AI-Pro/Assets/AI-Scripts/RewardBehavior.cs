using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class RewardBehavior : MonoBehaviour
{
    [SerializeField] private Transform playerTrans;
    [SerializeField] public Transform[] allGoalsTransform;
    [SerializeField] private Collider2D[] allToxics;
    

    private static RewardBehavior _self;
    private Collider2D _playerCollider;
    private float betweenPath;
    private int _curIndex;

    private void Awake()
    {
        if (_self == null)
            _self = this;
        _playerCollider = playerTrans.gameObject.GetComponent<Collider2D>();
    }

    private void Start()
    {
        _curIndex = 0;

    }

    public static RewardBehavior Shared()
    {
        return _self;
    }

    public Vector2 GetGoalPosition()
    {
        return allGoalsTransform[_curIndex].position;
    }

    public float SimpleReward()
    {
        var dist = Vector2.Distance(playerTrans.position, allGoalsTransform[_curIndex].position);
        var reward =  100 * math.exp(-dist/20);
        return reward;
    }

    public float L2Reward(PlayerState state)
    {
        var dist = Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position);
        // for (int i = _curIndex; i < allGoalsTransform.Length - 1; i++)
        // {
        //     dist += Vector2.Distance(allGoalsTransform[i].position, allGoalsTransform[i+1].position);
        // }
        return dist;
    }

    public bool IsCloseToGoal(PlayerState state)
    {
        return 100 > Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position);
    }

    public float pathReward()
    {
        float pathReward = 0;
        for (int i = _curIndex + 1; i < allGoalsTransform.Length - 1; i++)
        {
            pathReward -= 10;
        }

        return pathReward;
    }

    private float LossToxic()
    {
        var minColDist = allToxics.Min(x => x.Distance(_playerCollider));
        float minDist = minColDist.distance;
        print(minDist);
        return 50 * math.exp(-minDist/20);
    }

    public void IndexUp()
    {
        _curIndex++;
    }

    public float TotalReward()
    {
        
        return pathReward() + SimpleReward() - LossToxic();
    }
    
    
}
