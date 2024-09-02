using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;


public class AIHeuristic : MovementParent
{
    private static AIHeuristic _self;
    
    [Header("Heuristic"), SerializeField] private Tilemap bordersGrid;

    private Dictionary<string, float> _flowDict;
    


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _mainCam = Camera.main;
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
        if (_self == null)
            _self = this;
    }

    public static AIHeuristic Shared()
    {
        return _self;
    }
    
    private void Start()
    {
        _balloonSizeCur = 1;
        _goalBalloonSize = _balloonSizeCur;
        _lastTimeTouchedWall = Time.time;
        _flowDict = new Dictionary<string, float>();
        PreScan();
        StartCoroutine(AIUpdate());
    }

    private void PreScan()
    {
        PlayerState endState = new PlayerState(-370, 193);
        Queue<PlayerState> fringe = new Queue<PlayerState>();
        HashSet<string> visited = new HashSet<string>();
        float firstValue = 2000;
        _flowDict[StateToString(endState)] = firstValue;
        visited.Add(StateToString(endState));
        fringe.Enqueue(endState);
        while (fringe.Count > 0)
        {
            PlayerState cur = fringe.Dequeue();
            string stateAsStr = StateToString(cur);
            var nextStates = GetLegalActions(cur);
            foreach (var state in nextStates)
            {
                var nextAsString = StateToString(state);
                if (!visited.Contains(nextAsString))
                {
                    visited.Add(nextAsString);
                    fringe.Enqueue(state);
                    _flowDict[nextAsString] = _flowDict[stateAsStr] - 1f;
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
    
    private List<PlayerState> GetLegalActions(PlayerState state)
    {
        List<PlayerState> legal_actions = new List<PlayerState>();
        var x_movemoent = Enum.GetValues(typeof(AIMovement.XMovement));
        var y_movemoent = Enum.GetValues(typeof(AIMovement.YMovement));
        foreach (AIMovement.XMovement x_move in x_movemoent)
        {
            foreach (AIMovement.YMovement y_move in y_movemoent)
            {
                var cur_state = GetNextState(state, new PlayerAction(x_move, y_move));
                if(!IsTileAtPosition(state.GetAsVec())) 
                    legal_actions.Add(cur_state);
            }
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
        while (true)
        {
            yield return new WaitWhile(() => isChangingSize);
            var move = GetHeuristicMove();
            ResizeAndMove(move.Item1, move.Item2);
            yield return new WaitForSeconds(iterationTime);
        }
    }

    private (XMovement, YMovement) GetHeuristicMove()
    {
        return (XMovement.Left, YMovement.Up);
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
