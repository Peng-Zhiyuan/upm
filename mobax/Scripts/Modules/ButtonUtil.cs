using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;


public static class ButtonUtil 
{
    public static void SetClick(GameObject go, UnityAction action)
    {
        var button = go.GetOrAddComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    public static void SetClick(Component comp, UnityAction action)
    {
        SetClick(comp.gameObject, action);
    }


    [Obsolete("避免使用")]
    public static void ResetToggleListner(Toggle go, Action<Toggle, bool> action)
    {
        go.onValueChanged.RemoveAllListeners();
        go.onValueChanged.AddListener((bool isOn) =>
        {
            action?.Invoke(go, isOn);
        });
    }
}
