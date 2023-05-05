using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ToastManager
{
    public static void Show(string msg)
    {
        UIEngine.Stuff.RemoveFloating<ToastFloating>();
        var f = UIEngine.Stuff.ShowFloatingImediatly<ToastFloating>(wwise: "ui_warning");
        f.SetData(msg);
    }

    public static void ShowLocalize(string key)
    {
        var msg = LocalizationManager.Stuff.GetText(key);
        Show(msg);
    }

    public static void ShowLocalize(string key, params object[] argList)
    {
        var msg = LocalizationManager.Stuff.GetText(key, argList);
        Show(msg);
    }
}