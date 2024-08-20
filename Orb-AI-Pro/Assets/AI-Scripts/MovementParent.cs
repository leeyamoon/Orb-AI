using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovementParent : MonoBehaviour
{
    protected float _gravityRange;
    protected bool isChangingSize = false;
    #region Balloon properties
    protected float _verticalSpeed;
    protected float _horizontalSpeed;
    protected float _minBalloonSize;
    protected float _maxBalloonSize;
    protected float _sizeChangeSpeed;
    protected AnimationCurve _gravityCurve;
    protected float _minGravity;
    protected float _maxGravity; 
    protected float _changeWithScroller;
    protected float _scaleSpeed;
    protected float _moveHorizontal;
    protected float _moveVertical;
    protected float _balloonSizeCur;
    protected float _yAxisForceAmount;
    protected float _minForceAmount; 
    #endregion
    
    public void SetStage(StageProperties stage)
    {
        ChangeBalloonSizeOverTime(stage.minSize);
        AdvanceToStage(stage);
    }

    public void AdvanceToStage(StageProperties stage)
    {
        _minBalloonSize = stage.minSize;
        _maxBalloonSize = stage.maxSize;
        _verticalSpeed = stage.verticalSpeed;
        _horizontalSpeed = stage.horizontalSpeed;
        _maxGravity = stage.maxGravity;
        _minGravity = stage.minGravity;
        _gravityRange = _maxGravity - _minGravity;
        _scaleSpeed = stage.scaleSpeed;
        _changeWithScroller = stage.changeWithScroller;
        _gravityCurve = stage.gravityCurve;
        _yAxisForceAmount = stage.yAxisForceAmount;
        _minForceAmount = stage.minForceAmount;
    }
    
    private void ChangeBalloonSizeOverTime(float size)
    {
        isChangingSize = true;
        Vector3 targetScale = new Vector3(size, size, size);
        transform.DOScale(targetScale, 1f).SetEase(Ease.InOutQuad).
            OnComplete(()=>OnScaleComplete(size));
    }
    
    private void OnScaleComplete(float size)
    {
        _balloonSizeCur = size;
        isChangingSize = false;
    }
    
    public void SetBalloonSize(float size)
    {
        _balloonSizeCur = size;
    }
}
