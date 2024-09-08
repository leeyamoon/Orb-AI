using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroyerAfterTime : MonoBehaviour
{
    private float lifeTime;
    private Vector2 startVelocity;
    private Vector2 accelerationToGive;
    private float angularSpeed;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetValues(float lifeTimeM, Vector2 startVelocityM, Vector2 accelerationToGiveM, float angularSpeedM)
    {
        lifeTime = lifeTimeM;
        startVelocity = startVelocityM;
        accelerationToGive =accelerationToGiveM;
        angularSpeed = angularSpeedM;
    }

    private void Start()
    {
        _rigidbody.velocity = startVelocity;
        _rigidbody.angularVelocity = angularSpeed;
        StartCoroutine(SelfKiller());
    }

    private void FixedUpdate()
    {
        _rigidbody.velocity += accelerationToGive * Time.fixedDeltaTime;
    }

    private IEnumerator SelfKiller()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
