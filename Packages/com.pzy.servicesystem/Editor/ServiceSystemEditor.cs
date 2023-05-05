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

        var list = target.orderedServiceList;
        foreach(var one in list)
        {
            var serviceType = one.GetType();
            var typeName = serviceType.Name;
            var enbaled = one.enabled;
            enbaled = EditorGUILayout.Toggle(typeName, enbaled);
            one.enabled = enbaled;
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
