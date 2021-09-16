using UnityEngine;
using System;

public class PlayerPrefsUtil
{
    public static bool GetBool(string key, bool difault)
    {
        var v = PlayerPrefs.GetInt(key, -1);
        if(v == -1)
        {
            return difault;
        }
        return v == 1;
    }

    public static void SetBool(string key, bool value)
    {
        var v = value ? 1 : 0;
        PlayerPrefs.SetInt(key, v);
    }
}