using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SceneObject<T> : MonoBehaviour where T : SceneObject<T>
{
    private static T _stuff;
    public static T Stuff
    {
        get
        {
            if(_stuff == null)
            {
                // find exsists
                {
                    var go = GameObject.Find(typeof(T).Name);
                    if(go != null)
                    {
                        _stuff = go.GetComponent<T>();
                    }
                    else
                    {
                        throw new Exception($"SceneObject: {(typeof(T).Name)} not found in current scene" );
                    }
                }

            }
            return _stuff;
        }
    }
    
}
