using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using CustomLitJson;

public class RoleLocalCache
{
    private static string GenKey(string key)
    {
        var me = Database.Stuff.roleDatabase.Me;
        if (me == null)
        {
            throw new Exception("RoleLocalCache -> INTERNAL_ERROR : not Login");
        }

        var uid = me._id;
        var finalKey = $"{uid}.{key}";
        return finalKey;
    }

    /**
     * 获取数据
     */
    public static string GetString(string key, string defaultValue = null)
    {
        var finalKey = GenKey(key);
        if (PlayerPrefs.HasKey(finalKey))
        {
            return PlayerPrefs.GetString(finalKey);
        }
        else
        {
            return defaultValue;
        }
    }

    /**
     * 获取数据
     */
    public static int GetInt(string key, int defaultValue = 0)
    {
        var finalKey = GenKey(key);
        if (PlayerPrefs.HasKey(finalKey))
        {
            return PlayerPrefs.GetInt(finalKey);
        }
        else
        {
            return defaultValue;
        }
    }

    /**
     * 获取数据
     */
    public static float GetFloat(string key, float defaultValue = 0.0f)
    {
        var finalKey = GenKey(key);
        if (PlayerPrefs.HasKey(finalKey))
        {
            return PlayerPrefs.GetFloat(finalKey);
        }
        else
        {
            return defaultValue;
        }
    }


    /**
     * 保存数据
     * @param key 键
     * @param value 值 
     */
    public static void SetString(string key, string value)
    {
        var finalKey = GenKey(key);
        PlayerPrefs.SetString(finalKey, value);
    }

    /**
   * 保存数据
   * @param key 键
   * @param value 值 
   */
    public static void SetInt(string key, int value)
    {
        var finalKey = GenKey(key);
        PlayerPrefs.SetInt(finalKey, value);
    }

    /**
   * 保存数据
   * @param key 键
   * @param value 值 
   */
    public static void SetFloat(string key, float value)
    {
        var finalKey = GenKey(key);
        PlayerPrefs.SetFloat(finalKey, value);
    }

    /**
     * 删除数据
     * @param key 键
     */
    public static void Delete(string key)
    {
        var finalKey = GenKey(key);
        PlayerPrefs.DeleteKey(finalKey);
    }
}