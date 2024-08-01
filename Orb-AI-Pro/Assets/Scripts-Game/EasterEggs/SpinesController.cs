using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpinesController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform[] transformsLeft;
    [SerializeField] private Transform[] transformsRight;

    [SerializeField] private AnimationCurve curve;
    [SerializeField, Min(0.1f)] private float distToMoveFactor;
    [SerializeField, Min(0.1f)] private float radiusToEffect;

    private Vector3[] leftZeroPos;
    private Vector3[] rightZeroPos;

    private void Start()
    {
        leftZeroPos = transformsLeft.Select(x => x.position).ToArray();
        rightZeroPos = transformsRight.Select(x => x.position).ToArray();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if(!other.CompareTag("Player"))
            return;
        foreach (var transLeft in transformsLeft.Zip(leftZeroPos, (x,y) => (x,y)))
        {
            FitPosition(transLeft.x,transLeft.y, true);
        }
        foreach (var transRight in transformsRight.Zip(rightZeroPos, (x,y) => (x,y)))
        {
            FitPosition(transRight.x,transRight.y, false);
        }
    }

    private void FitPosition(Transform curTransform, Vector3 startPos, bool isLeft)
    {
        float yDist = Math.Abs(curTransform.position.y - playerTransform.position.y);
        if(radiusToEffect < yDist)
            return;
        var keyVal = 1 - (yDist / radiusToEffect);
        curTransform.position = startPos + curve.Evaluate(keyVal) * distToMoveFactor * Vector3.right * (isLeft ? -1:1);
    }
}
