using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stages")]
public class StageProperties : ScriptableObject
{
    [Min(0)] public int stageNumber;
    [Min(0)] public float minSize;
    [Min(0)] public float maxSize;
    public float verticalSpeed;
    public float horizontalSpeed;
    public AnimationCurve gravityCurve;
    public float minGravity;
    public float maxGravity;
    public float changeWithScroller;
    public float scaleSpeed;
    public float yAxisForceAmount;
    public float minForceAmount;
    
    // The following are currently not in use:
    public AnimationCurve forceSizeCurve;
    public float minForceFieldSize;
    public float maxForceFieldSize;
    public AnimationCurve forceValueCurve;
    public float minForceValueSize;
    public float maxForceValueSize; 
}
