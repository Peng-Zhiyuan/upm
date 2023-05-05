using UnityEngine;

public class LocalCache 
{
    public static string GAME = "sword";
    private static string GenKey(string key)
    {
        var finalKey = $"{GAME}.{key}";
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
        PlayerPrefs.DeleteKey($"{GAME}.{key}");
    }

    /**
    * 删除所有数据
    */
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    /**
   * 保存修改
   */
    public static void Save()
    {
        PlayerPrefs.Save();
    }
}