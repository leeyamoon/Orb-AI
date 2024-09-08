using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnerGate : MonoBehaviour
{
    [SerializeField] private GameObject gate;

    [SerializeField] private Vector2 finalGatePos;
    [SerializeField] private float amountOfSpines;
    [SerializeField] private bool canSpinOnlyClockwise;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _startGatePos;

    private float _curAngularCount;
    private float angularDest;
    private float _rotationLast;
    private float curRotation;
  


    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _startGatePos = gate.transform.position;
        angularDest = amountOfSpines * 360;
    }

    

    private void FixedUpdate()
    {
        if (canSpinOnlyClockwise)
            FreezeToClockwise();
        _curAngularCount += Math.Max(-_rigidbody2D.angularVelocity * Time.fixedDeltaTime, 0);
        gate.transform.position = Vector3.Lerp(_startGatePos, finalGatePos, _curAngularCount/angularDest);
    }

    private void FreezeToClockwise()
    {
        if (_rigidbody2D.angularVelocity > 0 && canSpinOnlyClockwise)
        {
            _rigidbody2D.freezeRotation = true;
        }
        else
        {
            _rigidbody2D.freezeRotation = false;
        }
    }

}
