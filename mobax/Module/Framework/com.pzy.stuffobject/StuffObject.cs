using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;

// 当拥有 ODIN_INSPECTOR 宏时，从 SerializedMonoBehaviour 继承以支持字典

#if ODIN_INSPECTOR
public class StuffObject<T> : SerializedMonoBehaviour where T : StuffObject<T>
#else
public class StuffObject<T> : MonoBehaviour where T : StuffObject<T>
#endif
{
    private static T _stuff;
    public static T Stuff
    {
        get
        {
            if (_stuff == null)
            {
                if (Application.isPlaying)
                {
                    var go = GameObject.Find(typeof(T).Name);
                    if (go != null)
                    {
                        _stuff = go.GetComponent<T>();
                    }
                    else
                    {
                        go = new GameObject();
                        DontDestroyOnLoad(go);
                        go.name = typeof(T).Name;
                        _stuff = go.AddComponent<T>();
                        _stuff.transform.SetParent(StuffRootHolder.StuffRoot.transform, false);
                    }
                }
                else
                {
                    throw new Exception("read Stuff in Editor mode");
                }
            }
            return _stuff;
        }
    }

    [Obsolete("此属性仅为遗留代码做适配，使用 Stuff 属性代替")]
    public static T Instance
    {
        get { return Stuff; }
    }
    

    public void Touch() { }

    public static void DestroyStuff()
    {
        if (_stuff != null)
        {
            GameObject.Destroy(_stuff.gameObject);
            _stuff = null;
        }
    }
}