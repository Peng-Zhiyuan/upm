using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(ServiceSystem))]
public class ServiceSystemEditor : Editor
{
    new ServiceSystem target;
    private void OnEnable()
    {
        this.target = base.target as ServiceSystem;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var list = target.serviceList;
        foreach(var one in list)
        {
            var serviceType = one.GetType();
            var typeName = serviceType.Name;

            var showName = "";
            if(IsEditorAssembly(one))
            {
                showName = $"[Editor] {typeName}";
            }
            else
            {
                showName = typeName;
            }
            EditorGUILayout.LabelField(showName);

        }

    }

    bool IsEditorAssembly(Service service)
    {
        var type = service.GetType();
        var assembly = type.Assembly;
        var assemblyName = assembly.GetName().Name;
        if(assemblyName == "Assembly-CSharp-Editor")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    string GetDescritionOfService(Service service)
    {
        var type = service.GetType();
        var attribute = type.GetCustomAttribute<ServiceDescriptionAttribute>();
        if(attribute == null)
        {
            return null;
        }
        return attribute.des;
    }
}
