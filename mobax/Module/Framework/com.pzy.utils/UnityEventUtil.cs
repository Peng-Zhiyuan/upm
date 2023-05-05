using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public static class UnityEventUtil
{
    /// <summary>
    /// 在编辑模式下添加可持久化的委托，或在运行模式下添加不可持久化的委托
    /// </summary>
    public static void AddVoidPersistentOrNotListener(this UnityEvent unityEvent, UnityAction action)
    {
        if (Application.isPlaying)
        {
            unityEvent.AddListener(action);
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(unityEvent, action);
#endif
        }

    }
}
