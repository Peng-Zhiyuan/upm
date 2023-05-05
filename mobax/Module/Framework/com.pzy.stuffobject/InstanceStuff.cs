using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;

// 当拥有 ODIN_INSPECTOR 宏时，从 SerializedMonoBehaviour 继承以支持字典

#if ODIN_INSPECTOR
public class InstanceStuff<T> : SerializedMonoBehaviour where T : InstanceStuff<T>
#else
public class StuffObject<T> : MonoBehaviour where T : StuffObject<T>
#endif
{
  
    public static T Create(Transform parent)
    {
        if (Application.isPlaying)
        {
            var go = new GameObject();
            DontDestroyOnLoad(go);
            go.name = typeof(T).Name;
            var comp = go.AddComponent<T>();
            comp.transform.SetParent(parent, false);
            return comp;
        }
        else
        {
            throw new Exception("read Stuff in Editor mode");
        }
    }
}
