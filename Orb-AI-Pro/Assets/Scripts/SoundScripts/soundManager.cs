using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class soundManager : MonoBehaviour
{

    [Header("----------- Audio Source -----------")] 
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource touchSoundSource;
    [SerializeField] private AudioSource blackHoleSource;
    [SerializeField] private AudioSource kickSource;
    [SerializeField] private AudioSource clickSource;
    [SerializeField] private AudioSource openDoorSource;
    [SerializeField] private AudioSource spinnerSource;
    [SerializeField] private AudioSource checkPointSource;
    [SerializeField] private AudioSource boingSource;

    [Header("----------- Audio Clip -----------")]
    public AudioClip BackGroundMusic;
    public AudioClip BlackHole;
    public AudioClip Touch;
    public AudioClip Kick;
    public AudioClip Click;
    public AudioClip OpenDoor;
    public AudioClip Spinner;
    public AudioClip CheckPoint;
    public AudioClip Boing;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioHighPassFilter BGHighPassFilter; 
    private const float LOW_PASS_DECREASE_SPEED = 100f;
    private const float START_BG_HIGH_PASS = 500f;

    private void Awake()
    {
        BGHighPassFilter.cutoffFrequency = START_BG_HIGH_PASS;
    }

    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = BackGroundMusic;
        musicSource.Play();
        
    }

    public void PlayEffect(AudioClip effect)
    {
        chooseAudioClip(effect).PlayOneShot(effect);
    }

    public void DecreaseLowPassFilter()
    {
        BGHighPassFilter.cutoffFrequency -= LOW_PASS_DECREASE_SPEED;
    }
    
    private AudioSource chooseAudioClip(AudioClip effect)
    {
        if (effect == Touch) return touchSoundSource;
        if (effect == BlackHole) return blackHoleSource;
        if (effect == Kick) return kickSource;
        if (effect == Click) return clickSource;
        if (effect == OpenDoor) return openDoorSource;
        if (effect == Spinner) return spinnerSource;
        if (effect == CheckPoint) return checkPointSource;
        if (effect == Boing) return boingSource;
        return musicSource;
    }
}