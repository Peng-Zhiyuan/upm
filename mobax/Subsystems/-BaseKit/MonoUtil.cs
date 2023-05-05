using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoUtil
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    public static GameObject CreateGameobject(string name)
    {
        GameObject go = new GameObject(name);
        return go;
    }

    public static T CreateGameobjectWithComponent<T>(string name) where T : Component
    {
        GameObject go = new GameObject(name);
        return go.GetOrAddComponent<T>();
    }

    public static RectTransform rectTransform(this MonoBehaviour mono)
    {
        return mono.GetComponent<RectTransform>();
    }
}