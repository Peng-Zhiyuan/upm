using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SimpleProgressBar : MonoBehaviour
{
    public Image bar;
    public bool enableColor;
    [ShowIf(nameof(enableColor))]
    public Color color = Color.white;
    [PropertyRange(0, 1)]
    public float progress;

    private const float Tolerance = .001f;
    private RectTransform _barRt;
    private int _length;
    private float _progress = -1;
    
    public void SetColor(Color newColor, float easeDuration = 0f)
    {
        color = newColor;
        if (easeDuration > 0)
        {
            bar.DOColor(color, easeDuration);
        }
        else
        {
            bar.color = color;
        }
    }
    
    public void SetProgress(float newProgress, bool ease = false)
    {
        newProgress = Math.Max(0, Math.Min(1, newProgress));
        if (Math.Abs(_progress - newProgress) < Tolerance) return;
        
        var newWidth = newProgress * _length;
        var size = new Vector2(newWidth, _barRt.sizeDelta.y);
        if (_progress < 0 || !ease)
        {
            _barRt.sizeDelta = size;
        }
        else
        {
            _barRt.DOSizeDelta(size, .2f + Math.Abs(newProgress - _progress) * .4f).SetEase(Ease.InCubic);
        }

        _progress = newProgress;
        progress = newProgress;
    }
    
    private void Awake()
    {
        _Initialized();
    }

    private void _Initialized()
    {
        _barRt = bar.GetComponent<RectTransform>();
        _length = (int) _barRt.sizeDelta.x;

        if (enableColor)
        {
            SetColor(color);
        }
    }
    
    #region 编辑器中表现
#if UNITY_EDITOR
    private bool _initializedInEditor;
    private void OnValidate()
    {
        if (!_initializedInEditor)
        {
            _Initialized();
            _initializedInEditor = true;
            return;
        }

        if (enableColor)
        {
            SetColor(color);
        }
        SetProgress(progress);
    }

    [ShowInInspector]
    private void ReInitialized()
    {
        _initializedInEditor = false;
        
        _Initialized();
        _progress = progress = 1;
        _initializedInEditor = true;
    }
#endif
    #endregion
}