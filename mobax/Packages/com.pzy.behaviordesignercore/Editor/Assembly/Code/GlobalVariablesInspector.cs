namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using UnityEditor;
    using UnityEngine;

    [UnityEditor.CustomEditor(typeof(BehaviorDesigner.Runtime.GlobalVariables))]
    public class GlobalVariablesInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Global Variabes", Array.Empty<GUILayoutOption>()))
            {
                GlobalVariablesWindow.ShowWindow();
            }
        }
    }
}

