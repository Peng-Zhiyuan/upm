using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResourcesObject<T> : MonoBehaviour where T : ResourcesObject<T>
{
    private static T _stuff;
    public static T Stuff
    {
        get
        {
            if(_stuff == null)
            {
                var go = GameObject.Find(typeof(T).Name);
                if (go != null)
                {
                    _stuff = go.GetComponent<T>();
                }
                else
                {
                    // �� Resource ����
                    var type = typeof(T);
                    var typeName = type.Name;
                    var prefab = Resources.Load<T>(typeName);
                    if (prefab == null)
                    {
                        throw new Exception($"[ResourcesObject] prefab: {typeName} not found in Resources");
                    }
                    //var stuffRoot = StuffRootHolder.StuffRoot;
                    var stuff = GameObject.Instantiate<T>(prefab);
                    stuff.name = typeName;
                    DontDestroyOnLoad(stuff);
                    _stuff = stuff;
                }
            }
            return _stuff;
        }
    }
}
