using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomPlayerAI : MovementParent
{

    private List<PlayerAction> _allActions;
    
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
        StartCoroutine(AIUpdate());
        LoadAllActions();
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
        while (true)
        {
            yield return new WaitWhile(() => isChangingSize);
            var action = _allActions[UnityEngine.Random.Range(0,_allActions.Count)];
            ResizeAndMove(action.moveX, action.moveY);
            yield return new WaitForSeconds(iterationTime);
        }
    }
    
    private void LoadAllActions()
    {
        _allActions = new List<PlayerAction>();
        var xMovement = Enum.GetValues(typeof(AIMovement.XMovement));
        var yMovement = Enum.GetValues(typeof(AIMovement.YMovement));
        foreach (AIMovement.XMovement xMove in xMovement)
        {
            foreach (AIMovement.YMovement yMove in yMovement)
            {
                var curAction = new PlayerAction(xMove, yMove); ;
                _allActions.Add(curAction);
            }
        }
    }
}
