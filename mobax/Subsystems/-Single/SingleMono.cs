using System;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance != null)
            {
                return s_Instance;
            }
            s_Instance = FindObjectOfType<T>();
            return s_Instance;
        }
    }

    private void OnDestroy()
    {
        s_Instance = null;
    }
}