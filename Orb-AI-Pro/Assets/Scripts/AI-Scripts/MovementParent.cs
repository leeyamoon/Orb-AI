using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovementParent : MonoBehaviour
{
    public enum XMovement
    {
        Left, Mid, Right
    }

    public enum YMovement
    {
        Up, Mid, Down
    }
    
    [SerializeField] protected float iterationTime;
    
    protected const float EPSILON = 0.1f;
    protected const string AUDIO_TAG = "Audio";
    protected const string WALL_TAG = "Wall";
    protected const string BOING = "Boing";
    protected const float THRESHOLD_FOR_INITIAL_MOVEMENT = 10f;
    protected float _goalBalloonSize;
    
    protected Camera _mainCam;

    
    protected float _forceRange;

    protected float _curGravity;


    protected float _totalMouseMovementX;
    protected bool shouldAllowMovement = false;
    protected float _lastTimeTouchedWall = 0f;
    protected soundManager _soundManager;
    
    protected Rigidbody2D _rigidbody;
    
    protected float _gravityRange;
    protected bool isChangingSize = false;
    #region Balloon properties
    protected float _verticalSpeed;
    protected float _horizontalSpeed;
    protected float _minBalloonSize;
    protected float _maxBalloonSize;
    protected float _sizeChangeSpeed;
    protected AnimationCurve _gravityCurve;
    protected float _minGravity;
    protected float _maxGravity; 
    protected float _changeWithScroller;
    protected float _scaleSpeed;
    protected float _moveHorizontal;
    protected float _moveVertical;
    protected float _balloonSizeCur;
    protected float _yAxisForceAmount;
    protected float _minForceAmount;
    [HideInInspector] public LocalEnv curPolicy;
    #endregion
    
    public void SetStage(StageProperties stage)
    {
        ChangeBalloonSizeOverTime(stage.minSize);
        AdvanceToStage(stage);
    }
    
    protected PlayerState get_state()
    {
        Vector3 objectPosition = transform.position;
        float x = objectPosition.x;
        float y = objectPosition.y;
        PlayerState state = new PlayerState(x, y);
        return state;
    }

    public void AdvanceToStage(StageProperties stage)
    {
        _minBalloonSize = stage.minSize;
        _maxBalloonSize = stage.maxSize;
        _verticalSpeed = stage.verticalSpeed;
        _horizontalSpeed = stage.horizontalSpeed;
        _maxGravity = stage.maxGravity;
        _minGravity = stage.minGravity;
        _gravityRange = _maxGravity - _minGravity;
        _scaleSpeed = stage.scaleSpeed;
        _changeWithScroller = stage.changeWithScroller;
        _gravityCurve = stage.gravityCurve;
        _yAxisForceAmount = stage.yAxisForceAmount;
        _minForceAmount = stage.minForceAmount;
    }
    
    private void ChangeBalloonSizeOverTime(float size)
    {
        isChangingSize = true;
        Vector3 targetScale = new Vector3(size, size, size);
        transform.DOScale(targetScale, 1f).SetEase(Ease.InOutQuad).
            OnComplete(()=>OnScaleComplete(size));
    }
    
    private void OnScaleComplete(float size)
    {
        _balloonSizeCur = size;
        isChangingSize = false;
    }
    
    public void SetBalloonSize(float size)
    {
        _balloonSizeCur = size;
    }
    
    
    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector2(_moveHorizontal * Time.fixedDeltaTime, _moveVertical));
        _moveHorizontal = 0;
        _moveVertical = 0;
    }
    

    protected void ForceChangeWithSize()
    {
        float curForce = -_minForceAmount + _yAxisForceAmount * _gravityCurve.Evaluate((_balloonSizeCur - _minBalloonSize)/
                                                                   (_maxBalloonSize-_minBalloonSize)) * _gravityRange;
        _rigidbody.AddForce(new Vector2(0, curForce * Time.deltaTime), ForceMode2D.Force);
    }


    protected void ChangeBalloonSizeAI(YMovement state)
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
    
    
    protected IEnumerator StepToGoalSizeAI()
    {
        while (Math.Abs(_goalBalloonSize - _balloonSizeCur) >= EPSILON)
        {
            if (_goalBalloonSize -_balloonSizeCur < -EPSILON)
                _balloonSizeCur -= Time.deltaTime * (1/iterationTime);
            else
                _balloonSizeCur += Time.deltaTime * (1/iterationTime);
            transform.localScale = Vector3.one * _balloonSizeCur;
            yield return null;
        }
    }

    protected void ResizeAndMove(XMovement stateX, YMovement stateY)
    {
        ChangeBalloonSizeAI(stateY);
        NewAxisMovement(stateX);
    }
    

    public float GetBalloonSize()
    {
        return _balloonSizeCur;
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

    protected void UpdateEnvironment(ref PlayerAction action)
    {
        if (curPolicy != null)
            action = curPolicy.GetAction();
    }
    
    
    protected void NewAxisMovement(XMovement stateX)
    /* Movement on X axis*/
    {
        float speedScale;
        switch (stateX)
        {
            case XMovement.Left:
                speedScale = -20;
                break;
            case XMovement.Right:
                speedScale = 20;
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
