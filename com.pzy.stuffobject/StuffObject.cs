using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class StuffObject<T> : SerializedMonoBehaviour where T : StuffObject<T>
{
    private static T _stuff;
    public static T Stuff
    {
        get
        {
            if(_stuff == null)
            {
                if(Application.isPlaying)
                {
                    var go = GameObject.Find(typeof(T).Name);
                    if(go != null)
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

    public void Touch()
    {

    }
}
