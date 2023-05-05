using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 带影子的进度条
/// </summary>
public class LauncherShadowProgressBar : MonoBehaviour
{
    public Color color = Color.white;
    public Image topBar;
    public Image shadowBar;
    [PropertyRange(0, 1)]
    public float progress;
    public RectOffset margin;

    private const float Tolerance = .001f;
    private string _flag;
    private float _progress = -1;
    private float _fullWidth;
    private RectTransform _topBarRt;
    private RectTransform _shadowBarRt;

    public void ResetProgress()
    {
        SetProgress(0);
    }

    public void SetColor(Color newColor, float easeDuration = 0f)
    {
        color = newColor;
        if (easeDuration > 0)
        {
            topBar.DOColor(newColor, easeDuration);
        }
        else
        {
            topBar.color = newColor;
        }
        // shadow的alpha降低
        newColor.a *= .4f;
        if (easeDuration > 0)
        {
            shadowBar.DOColor(newColor, easeDuration);
        }
        else
        {
            shadowBar.color = newColor;
        }
    }

    public void SetProgress(float newProgress, bool ease = false)
    {
        newProgress = Math.Max(0, Math.Min(1, newProgress));
        if (Math.Abs(_progress - newProgress) < Tolerance) return;
        
        var newWidth = newProgress * _fullWidth;
        var size = new Vector2(newWidth, _topBarRt.sizeDelta.y);
        if (_progress < 0 || !ease)
        {
            _topBarRt.sizeDelta = size;
            _shadowBarRt.sizeDelta = size;
        }
        else
        {
            if (newProgress > _progress)
            {
                _shadowBarRt.sizeDelta = size;
                _topBarRt.DOSizeDelta(size, .2f + (newProgress - _progress) * .4f).SetEase(Ease.InCubic);
            }
            else
            {
                _topBarRt.sizeDelta = size;
                _shadowBarRt.DOSizeDelta(size, .2f + (_progress - newProgress) * .4f).SetEase(Ease.InCubic);
            }
        }

        _progress = newProgress;
        progress = newProgress;
    }

    private void Awake()
    {
        _topBarRt = topBar.GetComponent<RectTransform>();
        _shadowBarRt = shadowBar.GetComponent<RectTransform>();
        
        var view = GetComponent<RectTransform>();
        _topBarRt.anchoredPosition = new Vector2(margin.left, _topBarRt.anchoredPosition.y);
        _shadowBarRt.anchoredPosition = new Vector2(margin.left, _shadowBarRt.anchoredPosition.y);
        _fullWidth = view.rect.width - margin.left - margin.right;
    }

    #region 编辑器中表现
#if UNITY_EDITOR
    private bool _initializedInEditor;
    private void OnValidate()
    {
        _DoInitialize();
        
        SetColor(color);
        SetProgress(progress);
    }

    private void _DoInitialize()
    {
        if (!_initializedInEditor)
        {
            _topBarRt = topBar.GetComponent<RectTransform>();
            _shadowBarRt = shadowBar.GetComponent<RectTransform>();
            
            var view = GetComponent<RectTransform>();
            _topBarRt.anchoredPosition = new Vector2(margin.left, _topBarRt.anchoredPosition.y);
            _shadowBarRt.anchoredPosition = new Vector2(margin.left, _shadowBarRt.anchoredPosition.y);
            _fullWidth = view.rect.width - margin.left - margin.right;
            _initializedInEditor = true;
        }
    }

    [ShowInInspector]
    private void DoReset()
    {
        _initializedInEditor = false;
        _DoInitialize();
        SetProgress(progress);
    }
#endif
    #endregion
}