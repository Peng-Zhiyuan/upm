using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class NG_Property 
{
    static public bool TryGetProperty(MaterialProperty[] properties, string name, out MaterialProperty property)
    {
        foreach (var p in properties)
        {
            if (p.name == name)
            {
                property = p;
                return true;
            }
        }
        property = null;
        return false;
    }

    static public  bool IsInCustomProperty(MaterialProperty property, params string[][] propertiesList)
    {
        foreach (var list in propertiesList)
        {
            foreach (var name in list)
            {
                if (name == property.name)
                {
                    return true;
                }
            }
        }
        return false;
    }

    static public bool IsInCustomProperty(MaterialProperty property, params string[] propertyList)
    {
        foreach (var name in propertyList)
        {
            if (name == property.name)
            {
                return true;
            }
        }
        return false;
    }



    static public void ShowProperty(string name, MaterialEditor materialEditor, MaterialProperty[] properties, string[] propertiesList)
    {
        if (!String.IsNullOrEmpty(name))
        {
            EditorGUILayout.LabelField(name, new GUIStyle(EditorStyles.boldLabel));
        }
        foreach (var p in properties)
        {
            if (IsInCustomProperty(p, propertiesList))
            {
                materialEditor.ShaderProperty(p, new GUIContent(p.displayName));
            }
        }
    }

}
