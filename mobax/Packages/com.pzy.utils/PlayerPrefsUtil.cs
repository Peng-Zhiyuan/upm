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

    public static void SetEnum<TEnum>(string key, TEnum value) where TEnum : Enum
    {
        var intValue = Convert.ToInt32(value);
        PlayerPrefs.SetInt(key, intValue);
    }

    public static TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : Enum
    {
        var b = PlayerPrefs.HasKey(key);
        if(!b)
        {
            return defaultValue;
        }
        var intValue = PlayerPrefs.GetInt(key);
        var v = (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        return v;
    }
}