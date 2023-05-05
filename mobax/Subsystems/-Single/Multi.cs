using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi<T> : Object where T : new()
{
    private static Dictionary<string, T> dic = new Dictionary<string, T>();
    protected string key = "";

    public static T GetInstance(string key)
    {
        if (!dic.ContainsKey(key))
        {
            dic[key] = new T();
            (dic[key] as Multi<T>).key = key;
        }
        return dic[key];
    }

    public static List<T> GetAll()
    {
        List<T> list = new List<T>();
        foreach (var key in dic.Keys)
        {
            list.Add(dic[key]);
        }
        return list;
    }

    public static void DestroyAll()
    {
        foreach (var key in dic.Keys)
        {
            dic[key] = default(T);
        }
        dic.Clear();
    }

    public static void Destroy(string key)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = default(T);
            dic.Remove(key);
        }
    }
}