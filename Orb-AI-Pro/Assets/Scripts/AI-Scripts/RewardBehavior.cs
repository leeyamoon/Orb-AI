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
    [SerializeField] public Collider2D[] allToxics;
    

    private static RewardBehavior _self;
    public Collider2D _playerCollider;
    private float betweenPath;
    public int _curIndex;
    private List<Vector2> lastPositions;
    private List<Vector2> lowVarPositios;

    public void Awake()
    {
        if (_self == null)
            _self = this;
        _playerCollider = playerTrans.gameObject.GetComponent<Collider2D>();
        _curIndex = 0;
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
        var reward = 100 / (dist + 1);
        return reward;
    }

    public float L2Reward(PlayerState state)
    {
        var dist = Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position);
        for (int i = _curIndex + 1; i < allGoalsTransform.Length - 2; i++)
        {
            dist +=  Vector2.Distance(allGoalsTransform[i+1].position, allGoalsTransform[i].position);
        }
        var shaprLoss = LossToxic() * 2;
        if (allToxics.Min(x => x.Distance(_playerCollider).distance) < 3)
            dist = dist * 10;
        var GoalsLeft = -pathReward();
        var varLoss = GetLocationVarianceLossSearch(state) / 2;
       // Debug.Log($"distnace: {dist}, sharp loss: {shaprLoss}, varLoss: {varLoss}, Goal index: {_curIndex}");
        return dist + shaprLoss + varLoss;
    }

    public float GetLocationVarianceLossSearch(PlayerState state)
    {
        if (lastPositions.Count == 40)
        {
            var first = lastPositions[0];
            lastPositions.Remove(first);    
        } 
        lastPositions.Add(state.GetAsVec());
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
        if (variance < 5)
        {
            foreach (var pos in lowVarPositios)
            {
                loss += math.exp(-Vector2.Distance(pos, playerTrans.position)/10);
            }
        }
        return loss;
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

    public float LossToxic()
    {
        var activeToxic = allToxics.Where(x => x.gameObject.activeSelf);
        var minDist = activeToxic.Min(x => x.Distance(_playerCollider).distance);
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
    
    //Exploring Algorithm
    // public float TotalReward(PlayerState state)
    // {  
    //     if (Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position) < 5)
    //     {
    //         return 100;
    //     }
    //     return -1;
    // }

    // Smart algorithm
    // public float TotalReward(PlayerState state)
    // {
    //     if (lastPositions.Count == 40)
    //     {
    //         var first = lastPositions[0];
    //         lastPositions.Remove(first);    
    //     } 
    //     lastPositions.Add(state.GetAsVec());
    //     var varLoss = GetLocationVarianceLoss();
    //     if (varLoss > 0){
    //         return pathReward() + 3 * SimpleReward() - LossToxic()/2 - varLoss;
    //     }
    //     return pathReward() + SimpleReward() - LossToxic() - 3*varLoss;
    // }

    //Merged algorithm
    public float TotalReward(PlayerState state)
    {   var tl = LossToxic() /4;
        if (Vector2.Distance(new Vector2(state.posX, state.posY), allGoalsTransform[_curIndex].position) < 5)
        {
            return 100 - tl;
        }
        return -1 - tl;
    }
    
    public float GetLocationVarianceLoss()
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
        return loss;
    }
    
}
