namespace DefaultNamespace;

using System;
using System.Collections.Generic;
using System.Linq;

public struct State
{
    public float posX;
    public float posY;

    public State(float posX, float posY)
    {
        this.posX = posX;
        this.posY = posY;
    }
}

public struct Action
{
    public XMovement moveX;
    public YMovement moveY;

    public Action(XMovement moveX, YMovement moveY)
    {
        this.moveX = moveX;
        this.moveY = moveY;
    }
}

public class QLearningAgent
{
    private Dictionary<(State, Action), double> qValues;
    private double epsilon; // exploration probability
    private double alpha; // learning rate
    private double discount; // discount rate

    public QLearningAgent(double epsilon, double alpha, double discount)
    {
        this.epsilon = epsilon;
        this.alpha = alpha;
        this.discount = discount;
        this.qValues = new Dictionary<(State, Action), double>();
    }

    public double GetQValue(State state, Action action)
    {
        var key = (state, action);
        return qValues.ContainsKey(key) ? qValues[key] : 0.0;
    }

    public double GetValue(State state)
    {
        var possibleActions = GetLegalActions(state);
        if (possibleActions.Count == 0)
        {
            return 0.0;
        }

        return possibleActions.Max(action => GetQValue(state, action));
    }

    public object GetPolicy(State state)
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

    public object GetAction(State state)
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

    public void Update(State state, Action action, State nextState, double reward)
    {
        var key = (state, action);
        var currentQValue = GetQValue(state, action);
        var nextValue = GetValue(nextState);
        qValues[key] = currentQValue + alpha * (reward + discount * nextValue - currentQValue);
    }

    private List<object> GetLegalActions(State state)
    {
        List<Action> legal_actions = new List<Action>();
        var x_movemoent = Enum.GetValues<XMovement>();
        var y_movemoent = Enum.GetValues<YMovement>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var action = Action((x_movemoent)i, (y_movemoent)j);
                legal_actions.Add(action);
            }
        }
        return legal_actions;
    }
}

public static class Util
{
    private static Random rand = new Random();

    public static bool FlipCoin(double probability)
    {
        return rand.NextDouble() < probability;
    }
}
