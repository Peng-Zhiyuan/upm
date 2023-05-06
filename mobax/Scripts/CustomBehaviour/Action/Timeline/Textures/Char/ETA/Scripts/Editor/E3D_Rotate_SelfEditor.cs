﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(E3D_Rotate_Self))]
public class E3D_Rotate_SelfEditor : Editor
{
    private string[] vfxCtrStr = new string[] { "X", "Y", "Z" };
    private E3D_Rotate_Self _target;
    private double _previousTime;
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_Rotate_Self)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);

        if (!Application.isPlaying && _target.useCurveMove == true)
            _target.SetUseCurve(false);
        if (!Application.isPlaying)
            _target.SaveOldTransform();
        _previousTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += InspectorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= InspectorUpdate;
        //编辑器下播放一半，失去焦点时恢复默认
        if (!Application.isPlaying && _target.useCurveMove)
        {
            if (_target != null)
                _target.SetOldTransform();
        }
        //对象停止播放时恢复默认
        if (!Application.isPlaying && _target._isPlay && _target.useCurveMove == false)
            _target.SetOldTransform();
    }

    //更新时间。
    private void InspectorUpdate()
    {
        //编辑器下Time的增量需要使用EditorApplication.timeSinceStartup 前后的差值
        var deltaTime = EditorApplication.timeSinceStartup - _previousTime;
        _previousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying && _target.useCurveMove)
        {
            _target.E3D_RotateSelf((float)deltaTime);
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
        //===============================================================================//

        EditorGUILayout.BeginVertical(GUI.skin.box);
        _target.times = EditorGUILayout.FloatField("Times：", _target.times);
        _target.loop = EditorGUILayout.Toggle("Loop：", _target.loop);
        _target.delayTime = EditorGUILayout.FloatField("Delay：", _target.delayTime);
        _target.showLine = EditorGUILayout.Toggle("Show Line: ", _target.showLine);

        _target.axisTypeIndex = EditorGUILayout.Popup("Self Axis: ", _target.axisTypeIndex, vfxCtrStr);
        _target.axis = (E3D_Rotate_Axis)_target.axisTypeIndex;

        _target.circles = EditorGUILayout.FloatField("Circles: ", _target.circles);
        _target.world = EditorGUILayout.Toggle("World Space: ", _target.world);
        EditorGUILayout.LabelField("Curve：");
        _target.curve = EditorGUILayout.CurveField("", _target.curve, GUILayout.Height(60), GUILayout.MinWidth(100));
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(_target._isPlaySelfRotation);
        if (GUILayout.Button("Play", GUILayout.MinWidth(100)))
        {
            _target._isPlaySelfRotation = true;
            //To Do
            _target._isPlay = true;
            //TargetReset();
            _target.DelayTimeRun();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Reset", GUILayout.MinWidth(100)))
        {
            _target._isPlaySelfRotation = false;
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

        EditorGUILayout.HelpBox("Tip:Default Local Space ", MessageType.Warning);

        if (EditorGUI.EndChangeCheck() || _target._isPlay)
        {
            EditorUtility.SetDirty(_target);//F2--重命名
        }
        //EditorUtility.SetDirty(_target);//F2--重命名
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
