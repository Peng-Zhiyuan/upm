using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class TransformUtil
{
    public static void RemoveAllChildren(Transform a)
    {
        if (a == null)
        {
            return;
        }
        for (int i = 0; i < a.childCount; i++)
        {
            var go = a.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                GameObject.Destroy(go);
            }
            else
            {
                GameObject.DestroyImmediate(go);
            }
        }
        a.DetachChildren();
    }

    public static void HideAllChildren(Transform t)
    {
        if(t == null)
        {
            return;
        }
        for (int i = 0; i < t.childCount; i++) 
        {
            Transform child = t.GetChild(i);
            child.gameObject.SetActive(false);
        }
    }
}