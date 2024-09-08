using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorBehavior : MonoBehaviour
{
    
    [SerializeField] private Sprite[] animationSprites;
    [SerializeField, Min(0.01f)] private float animationSpeed;
    [SerializeField] private float slowSpeed;
    

    private const string AUDIO_TAG = "Audio";
    private Image _image;
    private float curSpeed;
    private Camera _mainCam;
    private int _curSpriteIndex = 0;
    private float _spriteIndexFloat = 0;
    private soundManager _soundManager;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
        _mainCam = Camera.main;
        Cursor.visible = false;
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
    }
    
    
    private void Update()
    {
        transform.position = (Vector2)_mainCam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.mouseScrollDelta.y > 0)
        {
            _soundManager.PlayEffect(_soundManager.Click);
            curSpeed = -animationSpeed;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            _soundManager.PlayEffect(_soundManager.Click);
            curSpeed = animationSpeed;
        }
    }

    private void FixedUpdate()
    {
        UpdateCursorAnimSpeed();
        UpdateSprite();
    }

    private void UpdateCursorAnimSpeed()
    {
        if (!Mathf.Approximately(curSpeed, 0))
            curSpeed = Mathf.MoveTowards(curSpeed, 0, slowSpeed * Time.fixedDeltaTime);
    }

    private void UpdateSprite()
    {
        _spriteIndexFloat += curSpeed;
        if (_spriteIndexFloat >= animationSprites.Length)
            _spriteIndexFloat = 0;
        else if (_spriteIndexFloat < 0)
            _spriteIndexFloat = animationSprites.Length - 0.01f;
        _curSpriteIndex = (int)Math.Floor(_spriteIndexFloat);
        _image.sprite = animationSprites[_curSpriteIndex];
    }
}
