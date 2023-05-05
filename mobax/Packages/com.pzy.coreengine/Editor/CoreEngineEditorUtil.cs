using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public static class CoreEngineEditorUtil 
{
    public static object DrawObject(GUIContent guiContent, ICoreEngineSystemObject coreEngineObject, Type type)
    {
        if (typeof(CoreObject) == type)
        {
            var co = coreEngineObject as CoreObject;
            var debugerObj = co?.debbugerGameObject as GameObject;
            var postObj = EditorGUILayout.ObjectField(guiContent, debugerObj, typeof(CoreObjectDebuger), true) as CoreObjectDebuger;
            if(debugerObj != postObj)
            {
                var postDebuger = postObj?.GetComponent<CoreObjectDebuger>();
                return postDebuger?.co;
            }
            return co;
        }
        else if (typeof(CoreComponent) == type)
        {
            var comp = coreEngineObject as CoreComponent;
            var co = comp?.coreObject;
            var debugerObj = co?.debbugerGameObject as GameObject;
            var postObj = EditorGUILayout.ObjectField(guiContent, debugerObj, typeof(CoreObjectDebuger), true) as CoreObjectDebuger;
            if(postObj != debugerObj)
            {
                var postDebuger = postObj?.GetComponent<CoreObjectDebuger>();
                var postCo = postDebuger?.co;
                var newComp = postCo?.GetComponent(type);
                return newComp;
            }
            return comp;
        }
        else
        {
            throw new Exception("[CoreEngineEditorUtil] not support type: " + type.Name);
        }
    }
}
