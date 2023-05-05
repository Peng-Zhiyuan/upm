using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR 
[CustomEditor(typeof(E3D_BB))]
public class E3D_BBEditor : Editor
{
    private E3D_BB _target;
    private string[] vfxCtrStr = new string[] { "Lock Y", "Free" };
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_BB)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        //===============================================================================//
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", this.monoScript, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        //===============================================================================//

        EditorGUILayout.BeginVertical(GUI.skin.box);

        _target.billboardIndex = EditorGUILayout.Popup("Self Axies：", _target.billboardIndex, vfxCtrStr);
        _target.billboard = (E3D_BillBoard)_target.billboardIndex;
        _target.showLine = EditorGUILayout.Toggle("Show Line：", _target.showLine);
        _target.mainCamera = (Camera)EditorGUILayout.ObjectField("End Obj：", _target.mainCamera, typeof(Camera), true);

        if (GUILayout.Button("Help", GUILayout.MinWidth(100)))
        {
            Application.OpenURL("https://www.element3ds.com/thread-216743-1-1.html");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.HelpBox("使用注意！\n 1:Billboard 默认锁定Y轴", MessageType.Warning);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_target);//F2--重命名
        }
    }
}
#endif