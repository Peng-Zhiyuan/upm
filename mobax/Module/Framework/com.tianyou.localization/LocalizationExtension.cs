using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LocalizationExtension
{

    public static void SetLocalizer(this Text text, string key, params string[] argList)
    {        
        var localizer = text.gameObject.GetComponent<TextLocalizer>();
        if(localizer == null)
        {
            localizer = text.gameObject.AddComponent<TextLocalizer>();
        }
        localizer.Set(key, argList);
    }


    public static string Localize(this string key, params object[] paramList)
    {
        if (key == null)
        {
            return "";
        }
        else if (key == "")
        {
            return "";
        }
        var ret = LocalizationManager.Stuff.GetText(key, paramList);
        return ret;
    }
}
