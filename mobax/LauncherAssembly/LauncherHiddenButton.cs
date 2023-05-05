using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 开发者按钮
/// 看不见，连续点 5 此开启或者关闭开发者模式
/// </summary>
public class LauncherHiddenButton : MonoBehaviour, IPointerClickHandler
{
    public int clickCount = 15;

    float lastClickTime;
    float delta = 0.5f;
    int continuesClickCount;

    public UnityEvent onTriggerd;

    public void TryAddContinuesClick()
    {
        var now = Time.time;
        if(now - this.lastClickTime >= delta)
        {
            continuesClickCount = 0;
        }
        this.lastClickTime = now;
        continuesClickCount++;
        if(continuesClickCount >= this.clickCount)
        {
            continuesClickCount = 0;
            this.OnTrigger();
        }
    }

    public void OnTrigger()
    {
        onTriggerd?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.TryAddContinuesClick();
    }
}
