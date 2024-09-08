using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCloseDoor : MonoBehaviour
{
    [SerializeField] private Animator wallAnim;
    private const string AUDIO_TAG = "Audio";
    private const string PLAYER_TAG = "Player";
    private static readonly int IsOpen = Animator.StringToHash("IsOpen");
    private soundManager _soundManager;

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
    }
    

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag(PLAYER_TAG)) return;
        if(!wallAnim.GetBool(IsOpen)) _soundManager.PlayEffect(_soundManager.OpenDoor);
        wallAnim.SetBool(IsOpen, true);
    }
}
