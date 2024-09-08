using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Random = UnityEngine.Random;


public class ImportanceSampling
{
    private double epsilon;
    private double discount;
    private Dictionary<string, double> qValues;
    private Dictionary<string, double> weightDict;
    private Tilemap _bordersGrid;
    public ImportanceSampling(double epsilon, double discount, Tilemap bordersGrid)
    {
        this.epsilon = epsilon;
        this.discount = discount;
        qValues = new Dictionary<string, double>();
        weightDict = new Dictionary<string, double>();
        _bordersGrid = bordersGrid;
    }
    

    public List<(PlayerState, PlayerAction, double)> GenerateEpisode(int episode_size, PlayerState start_state)
    {
        PlayerState currentState = start_state;
        List<(PlayerState, PlayerAction, double)> episode = new List<(PlayerState, PlayerAction, double)>();
        for (int i=0;  i < episode_size; i++)
        {
            PlayerAction currentAction = GetAction(currentState);
            float reward = RewardBehavior.Shared().TotalReward(currentState);
            var Touple = (first: currentState, second: currentAction, third: reward);
            episode.Add(Touple);
            currentState = GetNextState(currentState, currentAction);

            // TODO: stop the episode when arriving to goal state
        }
        return episode;
    }

    public double GetWeight(PlayerState state, PlayerAction action)
    {
        var key = StateToString(state, action);
        if(!weightDict.ContainsKey(key))
        {
            return 0;
        }
        return weightDict[key];
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

    public PlayerAction GetPolicy(PlayerState state)
    {
        var legalActions = GetLegalActions(state);
        var bestAction = legalActions[0];
        double bestScore = GetQValue(state, legalActions[0]);
        foreach (var curAction in legalActions)
        {
            // Debug.Log($"{curAction.moveX} {curAction.moveY} {GetQValue(state, curAction)}");
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
        if (Util.FlipCoin(this.epsilon))
        {
            return legalActions[Random.Range(0,legalActions.Count)];
        }
        return GetPolicy(state);
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
                var action = new PlayerAction(x_move, y_move); //TODO
                //if(!IsTileAtPosition(GetNextState(state, action).GetAsVec())) 
                legal_actions.Add(action);
            }
        }
        return legal_actions;
    }
    
    private bool IsTileAtPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = _bordersGrid.WorldToCell(worldPosition);
        return _bordersGrid.HasTile(cellPosition);
    }

    public void UpdateQValues(List<(PlayerState, PlayerAction, double)> epsiode)
    {
        double GeneralReward = 0;
        double GeneralWeight = 1;
        Dictionary<string, double> muDict = qValues.ToDictionary(entry => entry.Key, entry => entry.Value);
        for (int i=epsiode.Count-2; i >= 0; i--)
        {
            PlayerState currentState = epsiode[i].Item1;
            PlayerAction currentAction = epsiode[i].Item2;
            double currentReward = epsiode[i+1].Item3;
            GeneralReward = GeneralReward * discount + currentReward;
            var stateActionKey = StateToString(currentState, currentAction);
            weightDict[stateActionKey] = GetWeight(currentState, currentAction) + GeneralWeight;
            qValues[stateActionKey] = GetQValue(currentState, currentAction) + (GeneralWeight/GetWeight(currentState, currentAction))*(GeneralReward - GetQValue(currentState, currentAction));
            GeneralWeight /=  MueActionState(currentState, currentAction, muDict);
        }
    }

    private double MueActionState(PlayerState state, PlayerAction action, Dictionary<string, double> muDict)
    {
        var legalActions = GetLegalActions(state);
        if (StateToString(state, GetPolicy(state)) == StateToString(state, action)) 
        {
            return 1 - this.epsilon + this.epsilon / legalActions.Count;
        }
        else 
        {
            return this.epsilon / legalActions.Count;
        }
    }

    public string StateToString(PlayerState state, PlayerAction action)
    {
        return "(" + state.posX + ", " + state.posY + ", " + action.moveX + ", " + action.moveY + ")";
    }
    
    // Define the function that takes a state and an action and returns the next state
    public PlayerState GetNextState(PlayerState currentState, PlayerAction action)
    {
        // Calculate the new X position based on the XMovement action
        int newPosX = currentState.posX;
        switch (action.moveX)
        {
            case AIMovement.XMovement.Left:
                newPosX -= 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.XMovement.Right:
                newPosX += 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.XMovement.Mid:
                // No change to X position
                break;
        }

        // Calculate the new Y position based on the YMovement action
        int newPosY = currentState.posY;
        switch (action.moveY)
        {
            case AIMovement.YMovement.Up:
                newPosY += 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.YMovement.Down:
                newPosY -= 1; // Adjust the value to match your movement logic
                break;
            case AIMovement.YMovement.Mid:
                // No change to Y position
                break;
        }

        // Return the new state with updated positions
        return new PlayerState(newPosX, newPosY);
    }
}


// public static class Util
// {
//     public static bool FlipCoin(double probability)
//     {
//         return Random.Range(0f,1f) < probability;
//     }
// }
