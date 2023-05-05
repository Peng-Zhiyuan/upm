using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 定制进度条
/// </summary>
public class DrawEliminationProgressBar : MonoBehaviour
{
    private const float Tolerance = .001f;
    private string _flag;

    public Color BarColor;
    public Image TopBar;
    public Image ShadowBar;
    [PropertyRange(0, 1)]
    public float Progress;

    private float _progress = -1;
    private float _fullWidth;
    private RectTransform _topBarRt;
    private RectTransform _shadowBarRt;

    public void ResetProgress()
    {
        SetProgress(0);
    }

    public void SetBarColor(Color color, float easeDuration = 0f)
    {
        BarColor = color;
        if (easeDuration > 0)
        {
            TopBar.DOColor(color, easeDuration);
        }
        else
        {
            TopBar.color = color;
        }
        // shadow的alpha降低
        color.a *= .4f;
        if (easeDuration > 0)
        {
            ShadowBar.DOColor(color, easeDuration);
        }
        else
        {
            ShadowBar.color = color;
        }
    }

    public void SetProgress(float progress, bool ease = false)
    {
        progress = Math.Max(0, Math.Min(1, progress));
        if (Math.Abs(_progress - progress) < Tolerance) return;
        
        var newWidth = progress * _fullWidth;
        var size = new Vector2(newWidth, _topBarRt.sizeDelta.y);
        if (_progress < 0 || !ease)
        {
            _topBarRt.sizeDelta = size;
            _shadowBarRt.sizeDelta = size;
        }
        else
        {
            if (progress > _progress)
            {
                _shadowBarRt.sizeDelta = size;
                _topBarRt.DOSizeDelta(size, .2f + (progress - _progress) * .4f).SetEase(Ease.InCubic);
            }
            else
            {
                _topBarRt.sizeDelta = size;
                _shadowBarRt.DOSizeDelta(size, .2f + (_progress - progress) * .4f).SetEase(Ease.InCubic);
            }
        }

        _progress = progress;
        Progress = progress;
    }

    private void Awake()
    {
        _topBarRt = TopBar.GetComponent<RectTransform>();
        _shadowBarRt = ShadowBar.GetComponent<RectTransform>();
        var padding = _topBarRt.offsetMin.x;
        _fullWidth = GetComponent<RectTransform>().rect.width - padding * 2;
    }

    #region 编辑器中表现
#if UNITY_EDITOR
    private bool _initializedInEditor;
    private void OnValidate()
    {
        if (!_initializedInEditor)
        {
            _topBarRt = TopBar.GetComponent<RectTransform>();
            _shadowBarRt = ShadowBar.GetComponent<RectTransform>();
            var padding = _topBarRt.offsetMin.x;
            _fullWidth = GetComponent<RectTransform>().rect.width - padding * 2;
            _initializedInEditor = true;
        }
        
        SetBarColor(BarColor);
        SetProgress(Progress);
    }
#endif
    #endregion
}