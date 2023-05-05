using System.Collections.Generic;
using UnityEngine;

public class MultiObject<T> : MonoBehaviour where T : MultiObject<T>
{
    private static Dictionary<string, T> dic = new Dictionary<string, T>();
    protected string key;
    private static T instance;

    public static T GetInstance(string key)
    {
        if (!dic.ContainsKey(key))
        {
            var go = new GameObject();
            DontDestroyOnLoad(go);
            go.name = typeof(T).Name;
            dic[key] = go.GetOrAddComponent<T>();
            if (instance == null)
            {
                instance = dic[key];
            }
            (dic[key] as MultiObject<T>).key = key;
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
            Destroy(dic[key].gameObject);
        }
        dic.Clear();
    }

    public static void Destroy(string key)
    {
        if (dic.ContainsKey(key))
        {
            Destroy(dic[key].gameObject);
            dic.Remove(key);
        }
    }
}