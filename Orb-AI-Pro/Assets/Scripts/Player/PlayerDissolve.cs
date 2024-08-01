using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerDissolve : MonoBehaviour
{
    [SerializeField] private Material playerMaterial;
    [SerializeField, Tooltip("Speed of dissolve effect")] private float dissolveSpeed = 0.5f;
    
    private const string ALPHA_PROPERTY = "_Alpha";
    

    public void Dissolve()
    {
        float currentAlpha = 0.8f;
        float duration = currentAlpha / dissolveSpeed;
        playerMaterial.DOFloat(0, ALPHA_PROPERTY, duration)
            .SetEase(Ease.OutCirc);
    }
    
    public void ReverseDissolve()
    {
        float duration = 1 / dissolveSpeed;
        playerMaterial.DOFloat(1, ALPHA_PROPERTY, duration).SetEase(Ease.OutCirc);
    }
}
