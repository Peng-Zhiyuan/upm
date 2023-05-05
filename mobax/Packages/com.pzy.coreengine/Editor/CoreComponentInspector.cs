using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;

public class CoreComponentInspector
{
    public CoreComponent target;
    public Editor debuggerUnityInspector;

    public virtual void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    public virtual void OnSceneGUI()
    {

    }

    public virtual void OnEnable()
    {

    }

    public virtual void OnDisable()
    {

    }

        
    public void Repaint()
    {
        this.debuggerUnityInspector.Repaint();
    }


    public void DrawDefaultInspector()
    {
        var comp = this.target;
        EditorGUI.BeginDisabledGroup(true);
        var scriptAsset = CoreComponentInspectorManager.GetScript(comp.GetType());
        EditorGUILayout.ObjectField("Script", scriptAsset, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        var type = comp.GetType();
        var fieldInfoList = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach(var f in fieldInfoList)
        {
            var fieldName = f.Name;
            if(fieldName == "instanceId")
            {
                continue;
            }
            else if(fieldName == "coreObject")
            {
                continue;
            }
            else if(fieldName == "debuggerUnityInspector")
            {
                continue;
            }
            else if (fieldName == "_isEnabled")
            {
                continue;
            }
            else if (fieldName == "isStartCalled")
            {
                continue;
            }
            var oldValue = f.GetValue(comp);
            var (newValue, isChanged) = DrawField(f, oldValue);
            if(isChanged)
            {
                f.SetValue(comp, newValue);
            }
        }
    }

    (object, bool) DrawField(FieldInfo f, object oldValue)
    {

        var fieldType = f.FieldType;
        var fieldName = f.Name;
        object newValue = null;

        EditorGUI.BeginChangeCheck();

        if (fieldType == typeof(int))
        {
            newValue = EditorGUILayout.IntField(fieldName, (int)oldValue);
        }
        else if(fieldType == typeof(float))
        {
            newValue = EditorGUILayout.FloatField(fieldName, (float)oldValue);
        }
        else if(fieldType == typeof(long))
        {
            newValue = EditorGUILayout.LongField(fieldName, (long)oldValue);
        }
        else if (fieldType == typeof(ulong))
        {
            var longValue = EditorGUILayout.LongField(fieldName, (long)oldValue);
            newValue = (ulong)longValue;
        }
        else if(fieldType == typeof(Vector3))
        {
            newValue = EditorGUILayout.Vector2Field(fieldName, (Vector3)oldValue);
        }
        else if(fieldType == typeof(Fixed))
        {
            var oldFloat = ((Fixed)oldValue).ToFloat();
            var newFloatValue = EditorGUILayout.FloatField(fieldName, oldFloat);
            var newFixed = new Fixed(newFloatValue);
            newValue = newFixed;
        }
        else if(fieldType == typeof(FixedVector3))
        {
            var oldVector3 = ((FixedVector3)oldValue).ToVector3();
            var newVector3 = EditorGUILayout.Vector3Field(fieldName, oldVector3);
            var newFixedVector3 = FixedVector3.FromVector3(newVector3);
            newValue = newFixedVector3;
        }
        else if(fieldType == typeof(bool))
        {
            newValue = EditorGUILayout.Toggle(fieldName, (bool)oldValue);
        }
        else if(fieldType.IsEnum)
        {
            newValue = EditorGUILayout.EnumPopup(fieldName, (Enum)oldValue);
        }
        else if(fieldType == typeof(CoreObject))
        {
            var co = oldValue as CoreObject;
            var debbugerGo = co?.debbugerGameObject as GameObject;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(fieldName, debbugerGo, typeof(GameObject), false);
            EditorGUI.EndDisabledGroup();
        }
        else if (typeof(CoreComponent).IsAssignableFrom(fieldType))
        {
            var comp = oldValue as CoreComponent;
            var co = comp?.coreObject;
            var debbugerGo = co?.debbugerGameObject as GameObject;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(fieldName, debbugerGo, typeof(GameObject), false);
            EditorGUI.EndDisabledGroup();
        }
        else if(typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
        {
            var unityObject = oldValue as UnityEngine.Object;
            newValue = EditorGUILayout.ObjectField(fieldName, unityObject, fieldType, true);
        }
        else if(fieldType == typeof(Quaternion))
        {
            var oldQ = (Quaternion)oldValue;
            var oldV = oldQ.eulerAngles;
            var newV = EditorGUILayout.Vector3Field(fieldName, oldV);
            var newQ = Quaternion.Euler(newV);
            newValue = newQ;
        }
        else if(fieldType == typeof(FixedQuaternion))
        {
            var oldFQ = (FixedQuaternion)oldValue;
            var oldQ = oldFQ.ToQuaternion();
            var oldV = oldQ.eulerAngles;
            var newV = EditorGUILayout.Vector3Field(fieldName, oldV);
            var newQ = Quaternion.Euler(newV);
            var newFQ = (FixedQuaternion)newQ;
            newValue = newFQ;
        }

        var isChanged = EditorGUI.EndChangeCheck();
        return (newValue, isChanged);
    }


}