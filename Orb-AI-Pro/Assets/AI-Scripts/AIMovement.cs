using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using MyBox.Internal;
using Unity.Mathematics;
using UnityEngine.Tilemaps;


public class AIMovement : MovementParent
{
    public enum XMovement
    {
        Left, Mid, Right
    }

    public enum YMovement
    {
        Up, Mid, Down
    }
    
    private const float EPSILON = 0.1f;
    private const string AUDIO_TAG = "Audio";
    private const string WALL_TAG = "Wall";
    private const string BOING = "Boing";
    private const float THRESHOLD_FOR_INITIAL_MOVEMENT = 10f;

    [SerializeField] private float iterationTime;
    [SerializeField] private Tilemap tilemap;
    
    private Camera _mainCam;
    private Rigidbody2D _rigidbody;
    
    private float _forceRange;

    private float _curGravity;

    private float _goalBalloonSize;
    private float _totalMouseMovementX;
    private bool shouldAllowMovement = false;
    private float _lastTimeTouchedWall = 0f;
    private soundManager _soundManager;
    private static AIMovement _self;

    [Header("QLearn parameters"), SerializeField, Range(0f,1f)]
    private float epsilon;
    [SerializeField, Range(0f,10f)] private float alpha;
    [SerializeField, Range(0f,1f)] private float discount;

    
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _mainCam = Camera.main;
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
        if (_self == null)
            _self = this;
    }

    public static AIMovement Shared()
    {
        return _self;
    }
    
    private void Start()
    {
        _balloonSizeCur = 1;
        _goalBalloonSize = _balloonSizeCur;
        _lastTimeTouchedWall = Time.time;
        StartCoroutine(AIUpdate());
        
    }
    

    private void Update()
    {
        if (GameManager.Shared()._isRespawning)
            return;
        ForceChangeWithSize();
    }

    // QLearning
    private IEnumerator AIUpdate()
    {
        QLearningAgent qAgent = new QLearningAgent(epsilon, alpha, discount);
        //ImportanceSampling ISAgent = new ImportanceSampling(epsilon, discount, tilemap);
        //LearnImportanceSampling(20, 1000, ISAgent);
        qAgent.load_qvalue_dict();
        StartCoroutine(AutoSave(qAgent));
        while (true)
        {
            yield return new WaitWhile(() => isChangingSize);
            //TODO add code transform.position
            q_learning_Step(qAgent);
            //PlayImportanceSampling(ISAgent);
            yield return new WaitForSeconds(iterationTime);
        }
        //qAgent.save_qvalue_dict();
    }

    // Graph Heuristic
    // private IEnumerator AIUpdate()
    // {
    //     List<List<PlayerAction>> actionsArr = new List<List<PlayerAction>>();
    //     AStar aStar = new AStar(get_state(), tilemap);
    //     List<PlayerAction> tempActionsArr = aStar.Search(RewardHeurisroc).Actions;
    //     actionsArr.Add(tempActionsArr);
    //     //RewardBehavior.Shared().IndexUp();
    //     for (int i = 0; i < RewardBehavior.Shared().allGoalsTransform.Length; i++)
    //     {
    //         Vector2 goalPos = RewardBehavior.Shared().allGoalsTransform[i].position; 
    //         aStar = new AStar(new PlayerState(goalPos.x, goalPos.y), tilemap);
    //         tempActionsArr = aStar.Search(RewardHeurisroc).Actions;
    //         actionsArr.Add(tempActionsArr);
    //         //RewardBehavior.Shared().IndexUp();
    //     }

    //     int goal_counter = 0; 
    //     // while (goal_counter <  RewardBehavior.Shared().allGoalsTransform.Length)
    //     // {
    //     //     var start_state = new PlayerState(0,0);
    //     //     if (goal_counter == 0)
    //     //     {
    //     //         start_state = get_state();
    //     //     }
    //     //     else
    //     //     {
    //     //         Vector2 goalPos = RewardBehavior.Shared().allGoalsTransform[goal_counter-1].position;
    //     //         start_state = new PlayerState(goalPos.x, goalPos.y);
    //     //     }
    //     //     AStar aStar = new AStar(start_state, tilemap);
    //     //     Node currentNode = aStar.Search(RewardHeurisroc);
    //     //     List<PlayerAction> tempActionsArr = currentNode.Actions;
    //     //     actionsArr.Add(tempActionsArr);
    //     //     var goal =  RewardBehavior.Shared().allGoalsTransform[goal_counter].position;
    //     //     if(IsPositionsClose(new Vector2(currentNode.currentState.posX, currentNode.currentState.posY), goal))
    //     //     {
    //     //         goal_counter++;
    //     //     }
    //     // }
    //     int arr_counter = 0;
    //     goal_counter = 0;
    //     while (arr_counter < actionsArr.Count)
    //     {
    //         foreach (var action in actionsArr[arr_counter])
    //         {
    //             yield return new WaitWhile(() => isChangingSize);
    //             var move = action;
    //             print(RewardHeurisroc(get_state()) + "   :  " + arr_counter + "  :  " + actionsArr[arr_counter].Count + "   :   " + RewardBehavior.Shared()._curIndex);
    //             ResizeAndMove(move.moveX, move.moveY);
    //             yield return new WaitForSeconds(iterationTime);
    //             var current_state = get_state();
    //             var goal =  RewardBehavior.Shared().allGoalsTransform[goal_counter].position;
    //             if(IsPositionsClose(new Vector2(current_state.posX, current_state.posY), goal))
    //             {
    //                 print("Got Close");
    //                 goal_counter++;
    //                 break;
    //             }
    //         }
    //         arr_counter++;
    //     }
    // }

    // private IEnumerator AIUpdate()
    // {
    //     while (true)
    //     {
    //         var current_state = get_state();
    //         var goal =  RewardBehavior.Shared().allGoalsTransform[RewardBehavior.Shared()._curIndex].position;
    //         // float numIter = Vector2.Distance(new Vector2(current_state.posX, current_state.posY),goal) / 4 + 3;
    //         float numIter = 14;
    //         AStar aStar = new AStar(get_state(), tilemap, numIter);
    //         List<PlayerAction> tempActionsArr = aStar.Search(RewardHeurisroc).Actions;
    //         foreach (var action in tempActionsArr)
    //         {
    //             yield return new WaitWhile(() => isChangingSize);
    //             var move = action;
    //             ResizeAndMove(move.moveX, move.moveY);
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

    private IEnumerator AutoSave(QLearningAgent qAgent)
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            qAgent.save_qvalue_dict();
        }
        
    }
    
    private double RewardHeurisroc(PlayerState state)
    {
        return RewardBehavior.Shared().L2Reward(state);
    }

    private bool IsPositionsClose(Vector2 x, Vector2 y)
    {
        return 10 > Vector2.Distance(x,y);
    }

    private PlayerState get_state()
    {
        Vector3 objectPosition = transform.position;
        float x = objectPosition.x;
        float y = objectPosition.y;
        PlayerState state = new PlayerState(x, y);
        return state;
    }
    
    // Define the function that takes a state and an action and returns the next state
    public PlayerState GetNextState(PlayerState currentState, PlayerAction action)
    {
        // Calculate the new X position based on the XMovement action
        int newPosX = currentState.posX;
        switch (action.moveX)
        {
            case XMovement.Left:
                newPosX -= 1; // Adjust the value to match your movement logic
                break;
            case XMovement.Right:
                newPosX += 1; // Adjust the value to match your movement logic
                break;
            case XMovement.Mid:
                // No change to X position
                break;
        }

        // Calculate the new Y position based on the YMovement action
        int newPosY = currentState.posY;
        switch (action.moveY)
        {
            case YMovement.Up:
                newPosY += 1; // Adjust the value to match your movement logic
                break;
            case YMovement.Down:
                newPosY -= 1; // Adjust the value to match your movement logic
                break;
            case YMovement.Mid:
                // No change to Y position
                break;
        }

        // Return the new state with updated positions
        return new PlayerState(newPosX, newPosY);
    }

    private void q_learning_Step(QLearningAgent agent)
    {
        PlayerState state = get_state();
        PlayerAction action = agent.GetAction(state);
        PlayerState next_state = GetNextState(state, action);
        float reward = RewardBehavior.Shared().TotalReward();
        agent.update(state, action, next_state, reward);
        ResizeAndMove(action.moveX, action.moveY);
    }

    private void LearnImportanceSampling(int num_episodes, int episode_size, ImportanceSampling ISAgent)
    {
        PlayerState state = get_state(); // make sure that this is the start state
        for (int i=0; i < num_episodes; i++)
        {
            var episode = ISAgent.GenerateEpisode(episode_size, state);
            ISAgent.UpdateQValues(episode);
        }
    }

    private void PlayImportanceSampling(ImportanceSampling ISAgent)
    {
        PlayerState state = get_state();
        PlayerAction action = ISAgent.GetPolicy(state);
        ResizeAndMove(action.moveX, action.moveY);
    }
    

    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector2(_moveHorizontal * Time.fixedDeltaTime, _moveVertical));
        _moveHorizontal = 0;
        _moveVertical = 0;
    }
    

    private void ForceChangeWithSize()
    {
        float curForce = -_minForceAmount + _yAxisForceAmount * _gravityCurve.Evaluate((_balloonSizeCur - _minBalloonSize)/
                                                                   (_maxBalloonSize-_minBalloonSize)) * _gravityRange;
        _rigidbody.AddForce(new Vector2(0, curForce * Time.deltaTime), ForceMode2D.Force);
    }


    private void ChangeBalloonSizeAI(YMovement state)
    {
        switch (state)
        {
            case YMovement.Up:
                _goalBalloonSize = _maxBalloonSize;
                break;
            case YMovement.Down:
                _goalBalloonSize = _minBalloonSize;
                break;
            default:
                _goalBalloonSize = (_maxBalloonSize + _minBalloonSize) / 2;
                break;
        }
        StartCoroutine(StepToGoalSizeAI());
    }
    
    
    private IEnumerator StepToGoalSizeAI()
    {
        while (Math.Abs(_goalBalloonSize - _balloonSizeCur) >= EPSILON)
        {
            if (_goalBalloonSize -_balloonSizeCur < -EPSILON)
                _balloonSizeCur -= Time.deltaTime * iterationTime * 2;
            else
                _balloonSizeCur += Time.deltaTime * iterationTime * 2;
            transform.localScale = Vector3.one * _balloonSizeCur;
            yield return null;
        }
    }

    private void ResizeAndMove(XMovement stateX, YMovement stateY)
    {
        ChangeBalloonSizeAI(stateY);
        NewAxisMovement(stateX);
    }
    

    public float GetBalloonSize()
    {
        return _balloonSizeCur;
    }
    
    
    private void NewAxisMovement(XMovement stateX)
    /* Movement on X axis*/
    {
        float speedScale;
        switch (stateX)
        {
            case XMovement.Left:
                speedScale = -30;
                break;
            case XMovement.Right:
                speedScale = 30;
                break;
            default:
                speedScale = 0;
                break;
        }
        float horizontalSpeed = _horizontalSpeed * speedScale;
        _moveHorizontal = horizontalSpeed;
    }

    #region Audio

        private void playTouchSound(Collision2D col)
        {
            if (col.gameObject.CompareTag(WALL_TAG))
            {
                if (Math.Abs(_lastTimeTouchedWall - Time.time) < EPSILON)
                {
                    _lastTimeTouchedWall = Time.time;
                    return;
                }
                _lastTimeTouchedWall = Time.time;
                _soundManager.PlayEffect(_soundManager.Touch);
            }
        }
    
        private void OnCollisionEnter2D(Collision2D col)
        {
            playTouchSound(col);
        }
    
        private void OnCollisionStay2D(Collision2D collision)
        {
            playTouchSound(collision);
        }
    
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag(BOING))
            {
                _soundManager.PlayEffect(_soundManager.Boing);
            }
        }

    #endregion

}
