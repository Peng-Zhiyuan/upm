using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class DebbugerButtonMenu 
{
    private const string PATH2 = "pzy.com.*/DeveloperMode";


    [MenuItem(PATH2)]
    public static void ToggleAutoFormat()
    {
        var b = DeveloperLocalSettings.IsDevelopmentMode;
        var newValue = !b;
        DeveloperLocalSettings.IsDevelopmentMode = newValue;
    }

    [MenuItem(PATH2, true)]
    public static bool ToggleAutoFormatValidate()
    {
        var isChecked = DeveloperLocalSettings.IsDevelopmentMode;
        Menu.SetChecked(PATH2, isChecked);
        return true;
    }
}
