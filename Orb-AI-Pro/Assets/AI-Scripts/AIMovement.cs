using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    enum XMovement
    {
        Left, Mid, Right
    }

    enum YMovement
    {
        Up, Mid, Down
    }
    
    private const float EPSILON = 0.1f;
    private const string AUDIO_TAG = "Audio";
    private const string WALL_TAG = "Wall";
    private const string BOING = "Boing";
    private const float THRESHOLD_FOR_INITIAL_MOVEMENT = 10f;

    [SerializeField] private float iterationTime;

    #region Balloon properties
        private float _verticalSpeed;
        private float _horizontalSpeed;
        private float _minBalloonSize;
        private float _maxBalloonSize;
        private float _sizeChangeSpeed;
        private AnimationCurve _gravityCurve;
        private float _minGravity;
        private float _maxGravity; 
        private float _changeWithScroller;
        private float _scaleSpeed;
        private float _moveHorizontal;
        private float _moveVertical;
        private float _balloonSizeCur;
        private float _yAxisForceAmount;
        private float _minForceAmount; 
    #endregion
    private Camera _mainCam;
    private Rigidbody2D _rigidbody;
    
    private float _gravityRange;
    private float _forceRange;

    private float _curGravity;

    private bool isChangingSize = false;

    private float _goalBalloonSize;
    private float _totalMouseMovementX;
    private bool shouldAllowMovement = false;
    private float _lastTimeTouchedWall = 0f;
    private soundManager _soundManager;
    
    
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
        print("Cor");
        StartCoroutine(AIUpdate());
    }

    public void SetStage(StageProperties stage)
    {
        ChangeBalloonSizeOverTime(stage.minSize);
        AdvanceToStage(stage);
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
            ResizeAndMove(XMovement.Right, YMovement.Up);
            yield return new WaitForSeconds(iterationTime);
        }
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

    public float GetBalloonSize()
    {
        return _balloonSizeCur;
    }

    public void SetBalloonSize(float size)
    {
        _balloonSizeCur = size;
    }
    
    
    private void NewAxisMovement(XMovement stateX)
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
        print(_moveHorizontal);
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
