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
    public int _curIndex;
    private List<Vector2> lastPositions;
    private List<Vector2> lowVarPositios;

    private void Awake()
    {
        if (_self == null)
            _self = this;
        _playerCollider = playerTrans.gameObject.GetComponent<Collider2D>();
        _curIndex = 0;
    }

    public void Start()
    {
        lastPositions = new List<Vector2>();
        lowVarPositios = new List<Vector2>();
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
        // var reward =  100 * math.exp(-dist/20);
        var reward = 100 / (dist + 1);
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
        return 10 > Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position);
    }

    public float pathReward()
    {
        float pathReward = 0;
        for (int i = _curIndex + 1; i < allGoalsTransform.Length - 1; i++)
        {
            pathReward -= 1;
        }

        return pathReward;
    }

    private float LossToxic()
    {
        var minDist = allToxics.Min(x => x.Distance(_playerCollider).distance);
        // return 10 * math.exp(-minDist/10);
        return 80 / (minDist+1);
    }

    public void IndexUp()
    {
        _curIndex++;
    }
    
    public void ChangeIndex(int i)
    {
        if(i < 0 || i >= allGoalsTransform.Length)
            return;
        _curIndex = i;
    }

    public float TotalReward(PlayerState state)
    {
        if (lastPositions.Count == 40)
        {
            var first = lastPositions[0];
            lastPositions.Remove(first);    
        } 
        lastPositions.Add(state.GetAsVec());
        var varLoss = 3 * getLocationVarianceLoss();
        if (varLoss > 0){
            Debug.Log($"{pathReward()} {SimpleReward()} {LossToxic()} {varLoss}");
            return pathReward() + 3 * SimpleReward() - LossToxic()/2 - varLoss;
        }
        return pathReward() + SimpleReward() - LossToxic() - varLoss;
    }
    
    public float getLocationVarianceLoss()
    {
        Vector2 avgPos = new Vector2(lastPositions.Average(x=>x.x),lastPositions.Average(x=>x.y));
        List<float> variances = new List<float>();
        foreach (var pos in lastPositions)
        {
            variances.Add(Vector2.Distance(pos, avgPos));
        }
        float variance = variances.Average(x=>x);
        if (variance < 5)
        {
            if (lowVarPositios.Count == 10)
            {
                var first = lowVarPositios[0];
                lowVarPositios.Remove(first);  
            }
            lowVarPositios.Add(avgPos);
        }
        float loss = 0;
        if (variance < 10)
        {
            foreach (var pos in lowVarPositios)
            {
                loss += math.exp(-Vector2.Distance(pos, playerTrans.position)/10);
            }
        }
        return loss;;
    }
}
