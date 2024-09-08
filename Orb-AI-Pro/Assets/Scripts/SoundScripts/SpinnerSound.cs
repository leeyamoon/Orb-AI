using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SpinnerSound : MonoBehaviour
{
    private const string AUDIO_TAG = "Audio";

    [SerializeField] AudioSource _spinnerAudio;
    [SerializeField] AudioMixer _audioMixer;
    [SerializeField] private Rigidbody2D[] allSpinnersRigidbody2D;
    private soundManager _soundManager;
    private const float _maxSpeed = 10f; // Maximum speed for maximum volume
    private const string VOLUME_PARAM = "SpinnerVolume"; 
    private  const float VOLUME_CHANGE_SPEED = 3f; // Control the rate of volume change
    private  const float MAX_VOLUME = -10f; // Control the rate of volume change
    private float currentVolumeDb = -80f;


    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
    }

    private void Start()
    {
        _spinnerAudio.loop = true;
        _spinnerAudio.clip = _soundManager.Spinner;
        _spinnerAudio.Play();
    }

    private void Update()
    {
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(allSpinnersRigidbody2D.Max(x => Mathf.Abs(x.angularVelocity))) / _maxSpeed);
        float targetVolumeDb = LinearToDecibel(normalizedSpeed);
        targetVolumeDb = Mathf.Min(MAX_VOLUME, targetVolumeDb);
        currentVolumeDb = Mathf.Lerp(currentVolumeDb, targetVolumeDb, Time.deltaTime * VOLUME_CHANGE_SPEED);
        _audioMixer.SetFloat(VOLUME_PARAM, currentVolumeDb);
     
    }
    
    private float LinearToDecibel(float linear)
    {
        return linear != 0 ? 20.0f * Mathf.Log10(linear) : -80.0f; 
    }
    
}
