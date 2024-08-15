using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class PlayerState
{
    public float posX;
    public float posY;

    public PlayerState(float posX, float posY)
    {
        this.posX = posX;
        this.posY = posY;
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
     private Dictionary<(PlayerState, PlayerAction), double> qValues;
    private double epsilon; // exploration probability
    private double alpha; // learning rate
    private double discount; // discount rate

    public QLearningAgent(double epsilon, double alpha, double discount)
    {
        this.epsilon = epsilon;
        this.alpha = alpha;
        this.discount = discount;
        this.qValues = new Dictionary<(PlayerState, PlayerAction), double>();
    }

    public double GetQValue(PlayerState state, PlayerAction action)
    {
        var key = (state, action);
        return qValues.ContainsKey(key) ? qValues[key] : 0.0;
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

    public object GetPolicy(PlayerState state)
    {
        var legalActions = GetLegalActions(state);
        if (legalActions.Count == 0)
        {
            return null;
        }

        var bestAction = legalActions
            .OrderByDescending(action => GetQValue(state, action))
            .FirstOrDefault();
        
        return bestAction;
    }

    public PlayerAction GetAction(PlayerState state)
    {
        var legalActions = GetLegalActions(state);
        if (legalActions.Count == 0)
        {
            return null;
        }

        if (Util.FlipCoin(epsilon))
        {
            return legalActions[new Random().Next(legalActions.Count)];
        }
        
        return GetPolicy(state);
    }
    

    public void Update(PlayerState state, PlayerAction action, PlayerState nextState, double reward)
    {
        var key = (state, action);
        var currentQValue = GetQValue(state, action);
        var nextValue = GetValue(nextState);
        qValues[key] = currentQValue + alpha * (reward + discount * nextValue - currentQValue);
    }

    private List<PlayerAction> GetLegalActions(PlayerState state)
    {
        List<PlayerAction> legal_actions = new List<PlayerAction>();
        var x_movemoent = Enum.GetValues<AIMovement.XMovement>();
        var y_movemoent = Enum.GetValues<AIMovement.YMovement>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var action = PlayerAction((x_movemoent)i, (y_movemoent)j);
                legal_actions.Add(action);
            }
        }
        return legal_actions;
    }

    public void load_qvalue_dict()
    {
        string file_path = "dictionary.json";
        if (File.Exists(file_path))
        {
            // Serialize the dictionary to a JSON string
            string jsonString = JsonSerializer.Serialize(dictionary);
            
            // Write the JSON string to a file
            File.WriteAllText(file_path, jsonString);
        }
    }

    public void save_qvalue_dict()
    {
        // Read the JSON string from the file
        string jsonString = File.ReadAllText("dictionary.json");

        // Deserialize the JSON string back to a dictionary
        this.qValues = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonString);
    }
}

public static class Util
{
    public static bool FlipCoin(double probability)
    {
        return Random.Range(0f,1f) < probability;
    }
}

