using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionalGate : MonoBehaviour
{
    [SerializeField] private GameObject[] spinners;

    [SerializeField] private float[] angles;

    [SerializeField] private float errorRange;

    private Animator wallAnim;
    private const string AUDIO_TAG = "Audio";
    
    private static readonly int IsOpen = Animator.StringToHash("IsOpen");
    private soundManager _soundManager;
    
    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectWithTag(AUDIO_TAG).GetComponent<soundManager>();
        wallAnim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(CheckInput());
    }
    

    private IEnumerator CheckInput()
    {
        bool allInRightAngle = false;
        while (!allInRightAngle)
        {
            allInRightAngle = spinners.Zip(angles, (x, y) => (x, y))
                .All((obj) => Mathf.Abs(obj.x.transform.rotation.eulerAngles.z - obj.y) < errorRange);
            yield return new WaitForSeconds(0.1f);
        }
        _soundManager.PlayEffect(_soundManager.OpenDoor);
        wallAnim.SetBool(IsOpen, true);
    }


    
    
    
    

    
    
    
}
