using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoldOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_FontAsset fontOnHover;
    private TMP_FontAsset originalFont;
    private TextMeshProUGUI _textMeshProUGUI;

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        originalFont = _textMeshProUGUI.font;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _textMeshProUGUI.font = fontOnHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _textMeshProUGUI.font = originalFont;
    }
}
