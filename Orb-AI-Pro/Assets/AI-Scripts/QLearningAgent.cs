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
    

    public void load_qvalue_dict()
    {
        if(!PlayerPrefs.HasKey("Qdict"))
            return;
        string json = PlayerPrefs.GetString("Qdict");
        SerializationWrapper<string, double> wrapper = JsonUtility.FromJson<SerializationWrapper<string, double>>(json);
        qValues = wrapper.items.ToDictionary(x => x.key, x => x.value);
    }
    
    public void save_qvalue_dict()
    {
        List<KeyValuePair<string, double>> kvpList = new List<KeyValuePair<string, double>>();
        foreach (var kvp in qValues)
        {
            kvpList.Add(new KeyValuePair<string, double>(kvp.Key, kvp.Value));
        }
        string json = JsonUtility.ToJson(new SerializationWrapper<string, double>(kvpList));
        PlayerPrefs.SetString("Qdict", json);
    }
}

public static class Util
{
    public static bool FlipCoin(double probability)
    {
        return Random.Range(0f,1f) < probability;
    }
}

[Serializable]
public class KeyValuePair<TK, TV>
{
    public TK key;
    public TV value;

    public KeyValuePair(TK key, TV value)
    {
        key = key;
        value = value;
    }
}

[Serializable]
public class SerializationWrapper<K, V>
{
    public List<KeyValuePair<K, V>> items;

    public SerializationWrapper(List<KeyValuePair<K, V>> new_items)
    {
        items = new_items;
    }
}

