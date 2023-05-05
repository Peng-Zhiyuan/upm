using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR 
[CustomEditor(typeof(E3D_Move_A_To_B))]
public class E3D_Move_A_To_BEditor : Editor
{
    private E3D_Move_A_To_B _target;
    private double _previousTime;
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_Move_A_To_B)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);

        if (!Application.isPlaying && _target.useCurveMove == true)
            _target.SetUseCurve(false);
        if (!Application.isPlaying)
            _target.SaveOldTransform();
        _previousTime = EditorApplication.timeSinceStartup;
        if (!Application.isPlaying)
            EditorApplication.update += InspectorUpdate;
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
            EditorApplication.update -= InspectorUpdate;

        if (!Application.isPlaying && _target.useCurveMove)
        {
            _target.SetOldTransform();
        }

        if (!Application.isPlaying && _target._isPlay && _target.useCurveMove == false)
            _target.SetOldTransform();
    }

    private void InspectorUpdate()
    {
        var deltaTime = EditorApplication.timeSinceStartup - _previousTime;
        _previousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying && _target.useCurveMove)
        {
            _target.Move_A2B((float)deltaTime);
            SceneView.RepaintAll();
            Repaint();
        }
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        //===============================================================================//
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", this.monoScript, typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        //==============================================================================//
        EditorGUILayout.BeginVertical(GUI.skin.box);

        _target.times = EditorGUILayout.FloatField("Times：", _target.times);
        _target.loop = EditorGUILayout.Toggle("Loop：", _target.loop);
        _target.delayTime = EditorGUILayout.FloatField("Delay：", _target.delayTime);
        _target.showLine = EditorGUILayout.Toggle("Show Line: ", _target.showLine);

        _target.lookAtDir = EditorGUILayout.Toggle("LookAt: ", _target.lookAtDir);
        _target.startObject = (GameObject)EditorGUILayout.ObjectField("Start Obj: ", _target.startObject, typeof(GameObject), true);
        _target.endObject = (GameObject)EditorGUILayout.ObjectField("End Obj: ", _target.endObject, typeof(GameObject), true);

        EditorGUILayout.LabelField("Curve: ");
        _target.curve = EditorGUILayout.CurveField("", _target.curve, GUILayout.Height(60), GUILayout.MinWidth(100));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play", GUILayout.MinWidth(100)))
        {
            _target._isPlay = true;
            //To Do
            TargetReset();
            _target.DelayTimeRun();
        }

        if (GUILayout.Button("Reset", GUILayout.MinWidth(100)))
        {
            _target._isPlay = false;
            //To Do
            TargetReset();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(_target._isPlay);
        if (GUILayout.Button("IC", GUILayout.MinWidth(100)))
        {
            _target.SaveOldTransform();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Help", GUILayout.MinWidth(100)))
        {
            Application.OpenURL("https://www.element3ds.com/thread-216743-1-1.html");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.HelpBox("Tip:\n From A To B \n ", MessageType.Warning);
        if (EditorGUI.EndChangeCheck() || _target._isPlay)
        {
            EditorUtility.SetDirty(_target);//F2--重命名
        }
    }
    private void TargetReset()
    {
        if (_target)
        {
            _target.SetUseCurve(false);
            _target.SetOldTransform();
        }
    }
}
#endif