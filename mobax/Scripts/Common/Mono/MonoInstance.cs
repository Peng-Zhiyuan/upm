using System;
using UnityEngine;

public class MonoInstance<T> : MonoBehaviour where T : Component
{
    public static T _ins;

    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                GameObject go = new GameObject("MonoInstance<" + typeof(T).Name + ">");
                DontDestroyOnLoad(go);
                _ins = go.AddComponent<T>();
            }
            return _ins;
        }
    }
}

public class Instace<T> where T : class, new()
{
    private static T _ins;

    public static T Ins
    {
        get { return _ins ?? (_ins = (T)Activator.CreateInstance(typeof(T))); }
    }
}