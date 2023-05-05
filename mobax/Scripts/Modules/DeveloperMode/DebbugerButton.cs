using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 开发者按钮
/// 看不见，连续点 5 此开启或者关闭开发者模式
/// </summary>
public class DebbugerButton : MonoBehaviour, IPointerDownHandler
{

    float lastClickTime;
    float delta = 0.5f;
    int continuesClickCount;

    public Func<Task<bool>> requestOpenDeveloperAsync;
    public Action statusChanged;

    public void TryAddContinuesClick()
    {
        var now = Time.time;
        if(now - this.lastClickTime >= delta)
        {
            continuesClickCount = 0;
        }
        this.lastClickTime = now;
        continuesClickCount++;
        if(continuesClickCount >= 15)
        {
            continuesClickCount = 0;
            this.OnTrigger();
        }
    }

    
    public async void OnTrigger()
    {
        var isDevelopmentMode = DeveloperLocalSettings.IsDevelopmentMode;
        if(isDevelopmentMode)
        {
            DeveloperLocalSettings.IsDevelopmentMode = false;
            statusChanged?.Invoke();
        }
        else
        {
            if(requestOpenDeveloperAsync == null)
            {
                DeveloperLocalSettings.IsDevelopmentMode = true;
                statusChanged?.Invoke();
            }
            else
            {
                var task = requestOpenDeveloperAsync.Invoke();
                var approve = await task;
                if(approve)
                {
                    DeveloperLocalSettings.IsDevelopmentMode = true;
                    statusChanged?.Invoke();
                }
            }
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        this.TryAddContinuesClick();
    }
}
