using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[DefaultExecutionOrder(-999)]
public class GameManager : MonoBehaviour
{
    private const string AUDIO_TAG = "Audio";
    private static GameManager _shared;
    
    [SerializeField] private StageProperties[] stages;
    [SerializeField] private Transform[] savePoints;
    [SerializeField] private AIMovement ball;
    [SerializeField] private float respawnTime = 1f;
    [SerializeField] private CursorBehavior cursorBehavior;
    [SerializeField] private CinemachineVirtualCamera staticCamera;
    [SerializeField] private Material staticObjMaterial;
    [SerializeField] private Material playerMaterial;
    [SerializeField] private Material toxicFillMaterial;
    [SerializeField] private Image fadeScreen;
    [SerializeField, Min(0.1f)] private float timeToFade;
    [SerializeField] private PlayerDissolve playerDissolve;
    [SerializeField, MinValue(0), MaxValue(2f)] private float delayBeforeReverseDissolve = 1f;

    [Header("TextFields"), SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI lostText;

    private int _stageNumber = 0;
    private int spawnPointIndex = 0;
    private Transform prevPoint;
    public bool _isRespawning = false;
    private soundManager _soundManager;
    
    private static int _winAmount = 0;
    private static int _lostAmount = 0;
    

    private void Awake()
    {
        if (_shared == null)
            _shared = this;
        else
            Destroy(gameObject);
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
        staticObjMaterial.color = Color.black;
        toxicFillMaterial.color = Color.black;
        playerMaterial.color = Color.black;
    }

    private void Start()
    {
        ball.SetStage(GetCurrentStage());
        StartCoroutine(FadeOutBlackScreen());
    }

    public StageProperties GetCurrentStage()
    {
        return stages[_stageNumber];
    }
    
    public void IncrementStageNumber()
    {
        if(_stageNumber < stages.Length)
            _stageNumber++;
    }

    public static GameManager Shared()
    {
        return _shared;
    }
    

    public void LoadNextLevel()
    {
        IncrementStageNumber();
        ball.AdvanceToStage(GetCurrentStage());
    }
    

    public void SetCurrentSpawnPoint(int newIndex)
    {
        spawnPointIndex = newIndex;
    }
    
    public void RespawnPlayer()
    {
        if (!_isRespawning) _soundManager.PlayEffect(_soundManager.Kick);
        ball.SetStage(GetCurrentStage());
        _isRespawning = true;
        ball.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        playerDissolve.Dissolve();
        //New For AI
        _lostAmount++;
        UpdateTextFields();
        //End New For AI
        ball.transform.DOMove(savePoints[spawnPointIndex].position, respawnTime)
            .OnComplete(() =>
            {
                _isRespawning = false;
            });
        float delay = respawnTime - delayBeforeReverseDissolve; // Delay before reverse
        DOVirtual.DelayedCall(delay, () =>
        {
            playerDissolve.ReverseDissolve(); // Reverse the dissolve
        });

        ball.SetBalloonSize((GetCurrentStage().maxSize - GetCurrentStage().minSize) / 2);
    }

    public CinemachineVirtualCamera GetStaticCamera()
    {
        return staticCamera;
    }

    public void TurnScrollerCursor(bool isOn)
    {
        cursorBehavior.gameObject.SetActive(isOn);
        Cursor.visible = !isOn;
    }

    private IEnumerator FadeOutBlackScreen()
    {
        fadeScreen.gameObject.SetActive(true);
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime * (1 / timeToFade);
            alpha = Math.Max(alpha, 0);
            fadeScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeScreen.gameObject.SetActive(false);
    }

    public void IncreaseLost()
    {
        _lostAmount++;
        UpdateTextFields();
    }

    public void IncreaseWon()
    {
        _winAmount++;
        UpdateTextFields();
    }

    private void UpdateTextFields()
    {
        winText.text = "Won: " + _winAmount.ToString();
        lostText.text = "Lost: " + _lostAmount.ToString();
    }
}
