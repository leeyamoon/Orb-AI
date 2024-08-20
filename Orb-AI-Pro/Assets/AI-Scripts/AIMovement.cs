using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


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

    private IEnumerator AIUpdate()
    {
        QLearningAgent qAgent = new QLearningAgent(epsilon, alpha, discount);
        //qAgent.load_qvalue_dict();
        while (true)
        {
            yield return new WaitWhile(() => isChangingSize);
            //TODO add code transform.position
            q_learning_Step(qAgent);
            yield return new WaitForSeconds(iterationTime);
        }
        //qAgent.save_qvalue_dict();
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
