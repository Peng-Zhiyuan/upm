using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameBaseSlideTabs<T> : MonoBehaviour, IPointerDownHandler where T : MonoBehaviour
{
    public Action<int> OnTabChanged;
    public Action<int> OnLockedTabClicked;
    [LabelText("不设置就会自动设置，即遍历去找该类型的所有组件")]
    public List<T> tabAnchors;
    [LabelText("是否点击选择")]
    public bool clickable = true;
    [LabelText("是否适配当前布局")]
    public bool flexible = false;
    
    public RectTransform slider;
    public Color textNormal = Color.grey;
    public Color textSelected = Color.white;

    private T _currentPicked;

    public int CurrentIndex { get; private set; } = -1;
    public int[] Disables { get; set; }
    
    public void SetDefault()
    {
        FocusOn(0, false);
    }

    public void FocusOn(int index, bool ease = true, bool force = false, bool clicked = false)
    {
        var graphic = tabAnchors[index];
        // 选中了同一个就不操作
        if (!force && _currentPicked == graphic) return;

        if (null != Disables && Array.IndexOf(Disables, index) >= 0)
        {
            OnLockedTabClicked?.Invoke(index);
            return;
        }

        if (null != _currentPicked)
        {
            _SetColor(_currentPicked, textNormal);
        }
        
        _SetColor(graphic, textSelected);
        _currentPicked = graphic;
        CurrentIndex = index;
        
        // 如果不在同一节点下， 那么需要做下坐标转化， 这个尚未做
        var pos = graphic.GetComponent<RectTransform>().anchoredPosition;
        if (ease)
        {
            slider.DOAnchorPos(pos, .2f);
        }
        else
        {
            slider.anchoredPosition = pos;
        }
        OnTabChanged?.Invoke(index);
        if (clicked)
        {
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_tabSwitching");
        }
    }

    public void FocusOn(T graphic, bool ease = true, bool force = false, bool clicked = false)
    {
        var index = tabAnchors.IndexOf(graphic);
        FocusOn(index, ease, force, clicked);
    }
    
    private void Awake()
    {
        _Initialize();
    }

    private void _Initialize()
    {
        if (null == tabAnchors || tabAnchors.Count <= 0)
        {
            tabAnchors = GetComponentsInChildren<T>().ToList();
            _SetException(gameObject, slider.gameObject);
        }
        
        foreach (var graphic in tabAnchors)
        {
            _SetColor(graphic, textNormal);
        }
        _DoFlexible();
    }

    private void _DoFlexible()
    {
        if (!flexible) return;
        if (null == tabAnchors || tabAnchors.Count <= 0) return;

        var containerWidth = this.GetComponent<RectTransform>().rect.width;
        var unitWidth = containerWidth / tabAnchors.Count;
        var fromX = -(containerWidth - unitWidth) / 2;
        slider.sizeDelta = new Vector2(unitWidth, slider.sizeDelta.y);
        for (var i = 0; i < tabAnchors.Count; i++)
        {
            var tab = tabAnchors[i];
            tab.GetComponent<RectTransform>().SetAnchoredPositionX(fromX + i * unitWidth);
        }
    }

    private void _SetException(params GameObject[] comps)
    {
        foreach (var comp in comps)
        {
            var sliderComp = comp.GetComponent<T>();
            if (null != sliderComp)
            {
                tabAnchors.Remove(sliderComp);
            }
        }
    }

    private void _SetColor(T comp, Color color)
    {
        if (comp is Graphic graphic)
        {
            graphic.color = color;
        }
        else if (comp is IColorable colorableComp)
        {
            colorableComp.SetColor(color);
        }
    }

    #region 点击响应
    private RectTransform _rectTransform; //控件所在画布

    private RectTransform CurrentRectTransform
    {
        get
        {
            if (null == _rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!clickable) return;
        
        Camera cam = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CurrentRectTransform, eventData.position, cam, out var downPos);
        T nearestGraphic = null;
        float minDistance = float.MaxValue;
        foreach (var graphic in tabAnchors)
        {
            var textPosition = graphic.GetComponent<RectTransform>().anchoredPosition;
            var newDistance = Vector2.Distance(downPos, textPosition);
            if (newDistance < minDistance)
            {
                minDistance = newDistance;
                nearestGraphic = graphic;
            }
        }
        
        FocusOn(nearestGraphic, clicked: true);
    }
    #endregion
    
#if UNITY_EDITOR
    public int selectIndex;
    
    private bool _initialized;
    private bool _activated;
    
    private void OnValidate()
    {
        if (!_activated)
        {
            return;
        }
        
        if (!_initialized)
        {
            _Initialize();
        }

        if (tabAnchors.Count > 0)
        {
            _initialized = true;

            selectIndex = Math.Max(0, Math.Min(selectIndex, tabAnchors.Count - 1));
            FocusOn(selectIndex, false, true);
        }
    }

    [ShowInInspector]
    private void Activate()
    {
        _initialized = false;
        tabAnchors = null;
        _activated = true;
        OnValidate();
    }
#endif
}