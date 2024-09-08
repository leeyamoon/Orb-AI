using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerBehaviour : MovementParent
{
    
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
    }
    

    private void Update()
    {
        if (GameManager.Shared()._isRespawning)
            return;
        if (!shouldAllowMovement)
        {
            _totalMouseMovementX += Mathf.Abs(Input.GetAxis("Mouse X"));
            if (_totalMouseMovementX > THRESHOLD_FOR_INITIAL_MOVEMENT)
                shouldAllowMovement = true;
            return;
        }
        if (!isChangingSize)
            ResizeScrollerMove();
        ForceChangeWithSize();
    }

    private void FixedUpdate()
    {
        _rigidbody.AddForce(new Vector2(_moveHorizontal * Time.fixedDeltaTime, _moveVertical));
        _moveHorizontal = 0;
        _moveVertical = 0;
    }
    

    private void ChangeBalloonSizeWithScroller(float amount)
    {
        _goalBalloonSize += amount * Time.deltaTime * _changeWithScroller;
        _goalBalloonSize = Mathf.Clamp(_goalBalloonSize, _minBalloonSize, _maxBalloonSize);
        StepToGoalSize();
        transform.localScale = Vector3.one * _balloonSizeCur;
    }

    private void StepToGoalSize()
    {
        if (Math.Abs(_goalBalloonSize - _balloonSizeCur) < EPSILON)
            return;
        if (_goalBalloonSize -_balloonSizeCur < -EPSILON)
            _balloonSizeCur -= Time.deltaTime * _scaleSpeed;
        else
            _balloonSizeCur += Time.deltaTime * _scaleSpeed;
    }

    private void ResizeScrollerMove()
    {
        ChangeBalloonSizeWithScroller(Input.mouseScrollDelta.y);
        NewAxisMovement();
    }
    

    private void OnScaleComplete(float size)
    {
        _balloonSizeCur = size;
        isChangingSize = false;
    }

    private void NewAxisMovement()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePosition = _mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the distance between the mouse and the object on the x-axis
        float distanceX = mousePosition.x - transform.position.x;

        // Calculate the movement speed based on the distance
        float speedScale = Mathf.Clamp01(Mathf.Abs(distanceX) / _mainCam.orthographicSize * _mainCam.aspect);
        float horizontalSpeed = _horizontalSpeed * speedScale;

        // Set the movement direction based on the mouse position
        _moveHorizontal = distanceX > 0 ? horizontalSpeed : -horizontalSpeed;
    }
}