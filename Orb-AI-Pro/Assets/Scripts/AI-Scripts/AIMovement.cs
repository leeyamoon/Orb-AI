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
    [SerializeField] private Tilemap tilemap;
    

    private static AIMovement _self;

    [Header("QLearn parameters"), SerializeField, Range(0f,1f)]
    private float epsilon;
    [SerializeField, Range(0f,10f)] private float alpha;
    [SerializeField, Range(0f,1f)] private float discount;

    private IEnumerator curAIUpdate;
    private QLearningAgent qAgent;

    
    
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
        qAgent = new QLearningAgent(epsilon, alpha, discount, tilemap);
        // ImportanceSampling ISAgent = new ImportanceSampling(0.1, discount, tilemap);
        // LearnImportanceSampling(20, 100, ISAgent);
        qAgent.load_qvalue_dict();
        StartCoroutine(AutoSave());
        StartCoroutine(AutoRestart());
        while (true)
        {
            yield return new WaitWhile(() => isChangingSize);
            //TODO add code transform.position
            q_learning_Step(qAgent);
            // PlayImportanceSampling(ISAgent);
            yield return new WaitForSeconds(iterationTime);
        }
        // qAgent.save_qvalue_dict();
    }

    
    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            qAgent.save_qvalue_dict();
        }
    }

    public void SaveQDict()
    {
        qAgent.save_qvalue_dict();
    }
    
    private IEnumerator AutoRestart()
    {
        while (true)
        {
            yield return new WaitForSeconds(605);
            GameManager.Shared().RespawnPlayer(false);
        }
    }
    

    private bool IsPositionsClose(Vector2 x, Vector2 y)
    {
        return 10 > Vector2.Distance(x,y);
    }
    

    private void q_learning_Step(QLearningAgent agent)
    {
        PlayerState state = get_state();
        PlayerAction action = curPolicy == null ? agent.GetAction(state) : curPolicy.GetForceAction();
        PlayerState nextState = GetNextState(state, action);
        float reward = RewardBehavior.Shared().TotalReward(nextState);
        agent.update(state, action, nextState, reward);
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
    

}
