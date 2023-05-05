using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.PlayerLoop;
namespace ScRender.Editor
{
    using Editor = UnityEditor.Editor;
    [CustomEditor(typeof(DLightNode)),CanEditMultipleObjects]
    public class DLightNodeInspector:Editor
    {
        private void OnEnable()
        {
            owner = (DLightNode) serializedObject.targetObject;
            var root = serializedObject.FindProperty("data");
            mainColor = root.FindPropertyRelative("mainColor");
            mainIntensity = root.FindPropertyRelative("mainintensity");
            shadowColor = root.FindPropertyRelative("shadowColor");
            shadowFalloff = root.FindPropertyRelative("shadowFallOff");

            finalDir = root.FindPropertyRelative("finalDir");
            shadowYZoffset = root.FindPropertyRelative("shadowYZoffset");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            //base.OnInspectorGUI();
            mainColor.colorValue = EditorGUILayout.ColorField(mainColorLabel, mainColor.colorValue);
            mainIntensity.floatValue=EditorGUILayout.FloatField(staticItensityLabel,mainIntensity.floatValue);
            
            shadowColor.colorValue = EditorGUILayout.ColorField(shadowColorLabel, shadowColor.colorValue);
            shadowFalloff.floatValue=EditorGUILayout.FloatField(shadowFalloffLabel,shadowFalloff.floatValue);
            
            finalDir.vector3Value = EditorGUILayout.Vector3Field(finalDirLabel, finalDir.vector3Value);

            shadowYZoffset.floatValue=EditorGUILayout.FloatField(shadowYZoffsettLabel,shadowYZoffset.floatValue);
            
            if (EditorGUI.EndChangeCheck())
            {
                //DoSomeThing;
                serializedObject.ApplyModifiedProperties();
                owner.ResetStatic();
            }
        }
        private DLightNode owner;
        private GUIContent mainColorLabel = new GUIContent("主灯光颜色");
        private SerializedProperty mainColor;
        private GUIContent staticItensityLabel = new GUIContent("主灯光强度");
        private SerializedProperty mainIntensity;

        private GUIContent finalDirLabel = new GUIContent("主灯光朝向");
        private SerializedProperty finalDir;

        private GUIContent shadowColorLabel = new GUIContent("阴影颜色");
        private SerializedProperty shadowColor;
        private GUIContent shadowFalloffLabel = new GUIContent("阴影衰减");
        private SerializedProperty shadowFalloff;
        
        

        private GUIContent shadowYZoffsettLabel = new GUIContent("阴影垂直度");
        private SerializedProperty shadowYZoffset;
    }
}