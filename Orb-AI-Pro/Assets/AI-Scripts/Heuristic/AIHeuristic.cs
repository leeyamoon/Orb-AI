using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.Mathematics;
using Random = UnityEngine.Random;


public class AIHeuristic : MovementParent
{

    [Header("Heuristic"), SerializeField] private Tilemap bordersGrid;
    [SerializeField, Range(0, 1)] private float epsilon;

    private Dictionary<string, float> _flowDict;

    private List<PlayerAction> _allActions;
    private List<Vector2> _lastPositions;
    


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _mainCam = Camera.main;
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
    }

    private void Start()
    {
        _balloonSizeCur = 1;
        _goalBalloonSize = _balloonSizeCur;
        _lastTimeTouchedWall = Time.time;
        _flowDict = new Dictionary<string, float>();
         _lastPositions = new List<Vector2>();
        LoadAllActions();
        PreScan();
        StartCoroutine(AIUpdate());
    }

    private void LoadAllActions()
    {
        _allActions = new List<PlayerAction>();
        var x_movemoent = Enum.GetValues(typeof(AIMovement.XMovement));
        var y_movemoent = Enum.GetValues(typeof(AIMovement.YMovement));
        foreach (AIMovement.XMovement x_move in x_movemoent)
        {
            foreach (AIMovement.YMovement y_move in y_movemoent)
            {
                var curAction = new PlayerAction(x_move, y_move); ;
                _allActions.Add(curAction);
            }
        }
    }

    private void PreScan()
    {
        PlayerState endState = new PlayerState(-370, 193);
        Queue<PlayerState> fringe = new Queue<PlayerState>();
        HashSet<string> visited = new HashSet<string>();
        float firstValue = 3000;
        _flowDict[StateToString(endState)] = firstValue;
        visited.Add(StateToString(endState));
        fringe.Enqueue(endState);
        while (fringe.Count > 0)
        {
            PlayerState cur = fringe.Dequeue();
            string stateAsStr = StateToString(cur);
            var nextStates = GetLegalNeighbors(cur);
            foreach (var state in nextStates)
            {
                var nextAsString = StateToString(state);
                if (!visited.Contains(nextAsString))
                {
                    visited.Add(nextAsString);
                    fringe.Enqueue(state);
                    _flowDict[nextAsString] = _flowDict[stateAsStr] - 1f - WallPenalty(state);
                }
            }
        }
        save_qvalue_dict();
    }
    
    public void save_qvalue_dict()
    {
        List<KeyValuePair<string, float>> kvpList = new List<KeyValuePair<string, float>>();
        foreach (var kvp in _flowDict)
        {
            kvpList.Add(new KeyValuePair<string, float>(kvp.Key, kvp.Value));
        }
        string json = JsonUtility.ToJson(new SerializationWrapper<string, float>(kvpList));
        SaveStringToFile(json, "FlowDict.txt");
    }
    
    void SaveStringToFile(string content, string fileName)
    {
        string path = Application.persistentDataPath + "/" + fileName;
        File.WriteAllText(path, content);
        Debug.Log("File saved to: " + path);
    }
    
    private List<PlayerState> GetLegalNeighbors(PlayerState state)
    {
        List<PlayerState> legal_actions = new List<PlayerState>();
        foreach (var action in _allActions)
        {
            var cur_state = GetNextState(state, action);
            if(!IsTileAtPosition(state.GetAsVec())) 
                legal_actions.Add(cur_state);
        }
        return legal_actions;
    }
    
    private List<PlayerAction> GetLegalActions(PlayerState state)
    {
        List<PlayerAction> legal_actions = new List<PlayerAction>();
        foreach (var action in _allActions)
        {
            if(!IsTileAtPosition(GetNextState(state, action).GetAsVec())) 
                legal_actions.Add(action);
        }
        return legal_actions;
    }
    
    private bool IsTileAtPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = bordersGrid.WorldToCell(worldPosition);
        return bordersGrid.HasTile(cellPosition);
    }
    
    private void Update()
    {
        if (GameManager.Shared()._isRespawning)
            return;
        ForceChangeWithSize();
    }

    private IEnumerator AIUpdate()
    {   
        int IterNum = 1;
        double counter = 0;
        while (true)
        {   
            // Debug.Log($"search numer {counter+1}");
            if (GetLocationVariance() < 5 || RewardBehavior.Shared().LossToxic() > 8)
            {
                // _lastPositions.Clear();
                IterNum = 1;
            }
            AStar aStar = new AStar(get_state(), bordersGrid, IterNum, counter * IterNum);
            List<PlayerAction> moves = aStar.Search(Heuristic).Actions;
            yield return new WaitWhile(() => isChangingSize);
            foreach (var move in moves)
            {
                // var move = GetHeuristicMove();
                //print(move.moveX + "  " + move.moveY);
                ResizeAndMove(move.moveX, move.moveY);
                AddLastPos();
            }
            yield return new WaitForSeconds(iterationTime);
            counter = counter + 1;
            IterNum = 1;
        }
    }

    private void AddLastPos()
    {
        if (_lastPositions.Count == 20)
        {
            _lastPositions.Remove(_lastPositions[0]);
        }
        _lastPositions.Add(get_state().GetAsVec());
    }

    private float GetLocationVariance()
    {   
        if (_lastPositions.Count < 5)
        {
            return 10;
        }
        Vector2 avgPos = new Vector2(_lastPositions.Average(x=>x.x),_lastPositions.Average(x=>x.y));
        List<float> variances = new List<float>();
        foreach (var pos in _lastPositions)
        {
            variances.Add(Vector2.Distance(pos, avgPos));
        }
        float variance = variances.Average(x=>x);
        return variance;
    }
    
    // aStar Search
    // private IEnumerator AIUpdate()
    // {
    //     //RewardBehavior.Shared().Start();
    //     while (true)
    //     {
    //         var current_state = get_state();
    //         var goal =  RewardBehavior.Shared().allGoalsTransform[RewardBehavior.Shared()._curIndex].position;
    //         // float numIter = Vector2.Distance(new Vector2(current_state.posX, current_state.posY),goal) / 4 + 3;
    //         float numIter = 5;
    //         AStar aStar = new AStar(get_state(), tilemap, numIter);
    //         List<PlayerAction> tempActionsArr = aStar.Search(RewardHeurisroc).Actions;
    //         foreach (var action in tempActionsArr)
    //         {
    //             yield return new WaitWhile(() => isChangingSize);
    //             var move = action;
    //             ResizeAndMove(move.moveX, move.moveY);
    //             if (RewardBehavior.Shared().allToxics.Min(x => x.Distance(RewardBehavior.Shared()._playerCollider).distance) < 10)
    //             {
    //                 numIter = 2;
    //                 break;
    //             }
    //             yield return new WaitForSeconds(iterationTime);
    //             current_state = get_state();
    //             // var goal =  RewardBehavior.Shared().allGoalsTransform[RewardBehavior.Shared()._curIndex].position;
    //             if(IsPositionsClose(new Vector2(current_state.posX, current_state.posY), goal))
    //             {
    //                 print("Got Close");
    //                 break;
    //             }
    //         }
    //     }
    // }
    private double Heuristic(PlayerState state)
    {   
        if(!_flowDict.ContainsKey(StateToString(state)))
            return 3000;
        Debug.Log(RewardBehavior.Shared().LossToxic());
        return 3000 - _flowDict[StateToString(state)] + RewardBehavior.Shared().LossToxic();
    }

    private PlayerAction GetHeuristicMove()
    {
        var curState = get_state();
        var bestAction = new PlayerAction(XMovement.Mid, YMovement.Mid);
        float bestValue = -1000;
        var actions = GetLegalActions(curState);
        // if (actions.Count == 0)
        //     return bestAction;
        // if (Util.FlipCoin(epsilon))
        // {
        //     return actions[Random.Range(0,actions.Count)];
        // }
        foreach (var action in actions)
        {
            string asString = StateToString(GetNextState(curState, action));
            if(!_flowDict.ContainsKey(asString))
                continue;
            if (_flowDict[asString] > bestValue)
            {
                bestValue = _flowDict[asString];
                bestAction = action;
            }
        }
        return bestAction;
    }

    private float WallPenalty(PlayerState state)
    {
        var legalStates = GetLegalNeighbors(state);
        if (legalStates.Count < 9)
            return 0.5f;
        foreach (var neighbor in legalStates)
        {
            if (GetLegalActions(neighbor).Count < 9)
                return 0.25f;
        }
        return 0;
    }
    

    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector2(_moveHorizontal * Time.fixedDeltaTime, _moveVertical));
        _moveHorizontal = 0;
        _moveVertical = 0;
    }
    
    public string StateToString(PlayerState state)
    {
        return "(" + state.posX + ", " + state.posY + ")";
    }
}
