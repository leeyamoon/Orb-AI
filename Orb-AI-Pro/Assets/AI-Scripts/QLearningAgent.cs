using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MyBox;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerState
{
    public int posX;
    public int posY;

    public PlayerState(float posX, float posY)
    {
        this.posX = IntSpace(posX, 1);
        this.posY = IntSpace(posY, 1);
    }

    public Vector2 GetAsVec()
    {
        return new Vector2(posX, posY);
    }

    private int IntSpace(float floatLoc, int factor)
    {
        return (int) math.round(floatLoc)/factor;
    }
}

public struct PlayerAction
{
    public AIMovement.XMovement moveX;
    public AIMovement.YMovement moveY;
    

    public PlayerAction(AIMovement.XMovement moveX, AIMovement.YMovement moveY)
    {
        this.moveX = moveX;
        this.moveY = moveY;
    }
}

// public class PlayerStateActionComparer : IEqualityComparer<(PlayerState, PlayerAction)>
// {
//     public bool Equals((PlayerState, PlayerAction) x, (PlayerState, PlayerAction) y)
//     {
//         return x.Item1.posX == y.Item1.posX && x.Item1.posY == y.Item1.posY && x.Item2.moveX == y.Item2.moveX && x.Item2.moveY == y.Item2.moveY;
//     }
//
//     public int GetHashCode((PlayerState, PlayerAction) obj)
//     {
//         unchecked // Overflow is fine, just wrap
//         {
//             int hash = 17;
//             hash = hash * 23 + obj.Item1.GetHashCode();
//             hash = hash * 23 + obj.Item2.GetHashCode();
//             return hash;
//         }
//     }
// }


public class QLearningAgent
{
    private Dictionary<string, double> qValues;
    private double epsilon; // exploration probability
    private double alpha; // learning rate
    private double discount; // discount rate

    public QLearningAgent(double epsilon, double alpha, double discount)
    {
        this.epsilon = epsilon;
        this.alpha = alpha;
        this.discount = discount;
        qValues = new Dictionary<string, double>();
    }

    public double GetQValue(PlayerState state, PlayerAction action)
    {
        var key = StateToString(state, action);
        if(!qValues.ContainsKey(key))
        {
            var actions = GetLegalActions(state);
            var destVec = RewardBehavior.Shared().GetGoalPosition() - state.GetAsVec();
            var destVecNorm = destVec.normalized;
            foreach (var playerAction in actions)
            {
                var nextState = AIMovement.Shared().GetNextState(state, playerAction);
                var changeWithStep = (nextState.GetAsVec() - state.GetAsVec()).normalized;
                float angle = changeWithStep[0] * destVecNorm[0] + changeWithStep[1] * destVecNorm[1];
                var value = -angle * destVec.magnitude;
                qValues[StateToString(state, playerAction)] = value;
            }
        }
        return qValues[key];
    }

    public double GetValue(PlayerState state)
    {
        var possibleActions = GetLegalActions(state);
        if (possibleActions.Count == 0)
        {
            return 0.0;
        }

        return possibleActions.Max(action => GetQValue(state, action));
    }

    public PlayerAction GetPolicy(PlayerState state)
    {
        var legalActions = GetLegalActions(state);
        var bestAction = legalActions[0];
        double bestScore = GetQValue(state, legalActions[0]);
        foreach (var curAction in legalActions)
        {
            if (bestScore > GetQValue(state, curAction))
            {
                bestScore = GetQValue(state, curAction);
                bestAction = curAction;
            }
        }
        return bestAction;
    }

    public PlayerAction GetAction(PlayerState state)
    {
        var legalActions = GetLegalActions(state);
        if (Util.FlipCoin(epsilon))
        {
            return legalActions[Random.Range(0,legalActions.Count)];
        }
        return GetPolicy(state);
    }
    

    public void update(PlayerState state, PlayerAction action, PlayerState nextState, double reward)
    {
        var currentQValue = GetQValue(state, action);
        var nextValue = GetValue(nextState);
        qValues[StateToString(state, action)] = currentQValue + alpha * (reward + discount * nextValue - currentQValue);
        
    }

    public string StateToString(PlayerState state, PlayerAction action)
    {
        return "(" + state.posX + ", " + state.posY + ", " + action.moveX + ", " + action.moveY + ")";
    }

    private List<PlayerAction> GetLegalActions(PlayerState state)
    {
        List<PlayerAction> legal_actions = new List<PlayerAction>();
        var x_movemoent = Enum.GetValues(typeof(AIMovement.XMovement));
        var y_movemoent = Enum.GetValues(typeof(AIMovement.YMovement));
        foreach (AIMovement.XMovement x_move in x_movemoent)
        {
            foreach (AIMovement.YMovement y_move in y_movemoent)
            {
                var action = new PlayerAction(x_move, y_move);
                legal_actions.Add(action);
            }
        }
        return legal_actions;
    }
    

    // public void load_qvalue_dict()
    // {
    //     string file_path = "dictionary.json";
    //     if (File.Exists(file_path))
    //     {
    //         // Serialize the dictionary to a JSON string
    //         string jsonString = JsonSerializer.Serialize(dictionary);
    //         
    //         // Write the JSON string to a file
    //         File.WriteAllText(file_path, jsonString);
    //     }
    // }
    //
    // public void save_qvalue_dict()
    // {
    //     // Read the JSON string from the file
    //     string jsonString = File.ReadAllText("dictionary.json");
    //
    //     // Deserialize the JSON string back to a dictionary
    //     this.qValues = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);
    // }
}

public static class Util
{
    public static bool FlipCoin(double probability)
    {
        return Random.Range(0f,1f) < probability;
    }
}

