using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(UIEngine))]
public class UIEngineEditor : Editor
{
    UIEngine Component
    {
        get
        {
            return target as UIEngine;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(!Application.isPlaying)
        {
            return;
        }
        var list = Component.pageStack.InnerStack.ToArray();
        // stack
        EditorGUILayout.PrefixLabel("Stack");
       // EditorGUI.indentLevel++;
        EditorGUI.BeginDisabledGroup(true);
        for(var i = 0; i < list.Length; i++)
        {
            //var page = list[i];
            //var hotCreator = page.creator;
            //var hotCreatorType = typeof(HotBehaviorCreator);
            //var index = list.Length - 1 - i;
            //EditorGUILayout.ObjectField(index.ToString(), hotCreator, hotCreatorType, true);
            var page = list[i];
            var hotCreator = page.GameObject;
            var hotCreatorType = typeof(GameObject);
            var index = list.Length - 1 - i;
            EditorGUILayout.ObjectField(index.ToString(), hotCreator, hotCreatorType, true);
        }
        
        // EditorGUILayout.Separator();
        // EditorGUILayout.Separator();
        //EditorGUI.indentLevel--;
        //EditorGUILayout.PrefixLabel("Pool");
        // EditorGUILayout.Separator();
        // EditorGUILayout.PrefixLabel("None");
        

        // pool
        // foreach(var kv in Component.pagePool.dic)
        // {
        //     var name = kv.Key;
        //     var instanceQueue = kv.Value;
        //     var firstPage = instanceQueue.Peek();
        //     EditorGUILayout.ObjectField(name, firstPage, typeof(Page), false);

        // }


        EditorGUI.EndDisabledGroup();

        GUILayout.Button("Refresh");

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        serializedObject.ApplyModifiedProperties ();


    }

}
