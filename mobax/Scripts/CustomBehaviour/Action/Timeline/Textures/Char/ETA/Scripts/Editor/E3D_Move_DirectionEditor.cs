﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(E3D_Move_Direction))]
public class E3D_Move_DirectionEditor : Editor
{
    private E3D_Move_Direction _target;
    private double _previousTime;
    private string[] _enumStr = new string[] { "X", "Y", "Z", "-X", "-Y", "-Z" };
    private Color guiIdlebackgroundColor;
    private Color backgroundColor_Blue = new Color(0, 1, 204 / 255, 0.4f);
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_Move_Direction)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);

        if (!Application.isPlaying && _target.useCurveMove == true)
            _target.SetUseCurve(false);
        if (!Application.isPlaying)
            _target.SaveOldTransform();
        _previousTime = EditorApplication.timeSinceStartup;
        if (!Application.isPlaying)
            EditorApplication.update += InspectorUpdate;
        guiIdlebackgroundColor = GUI.backgroundColor;
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
            EditorApplication.update -= InspectorUpdate;

        //编辑器下播放一半，失去焦点时恢复默认
        if (!Application.isPlaying && _target.useCurveMove)
        {
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

        if (!Application.isPlaying && _target.useCurveMove == true)
        {
            _target.MoveDirection((float)deltaTime);
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

        _target.intensity = EditorGUILayout.FloatField("Instensity: ", _target.intensity);
        _target.world = EditorGUILayout.Toggle("World Space: ", _target.world);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = backgroundColor_Blue;
        EditorGUI.indentLevel += 1;
        if (_target.world)
        {
            _target.direction = EditorGUILayout.Vector3Field("Move Dir: ", _target.direction);
            _target.lookAtDir = EditorGUILayout.Toggle("LookAt: ", _target.lookAtDir);
            _target.lookAtSpeed = EditorGUILayout.FloatField("LookAt Speed: ", _target.lookAtSpeed);
        }
        else
        {
            _target.enumIndex = EditorGUILayout.Popup("Local Dir:", _target.enumIndex, _enumStr);
            _target.moveEnum = (E3D_Move_Enum)_target.enumIndex;
        }
        EditorGUI.indentLevel -= 1;
        _target._startLocalPos = EditorGUILayout.Vector3Field("StartLocalPos: ", _target._startLocalPos);
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = guiIdlebackgroundColor;
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

        EditorGUILayout.HelpBox("Tip:\n Default Local Direction ", MessageType.Warning);

        if (EditorGUI.EndChangeCheck() || _target._isPlay)
        {
            EditorUtility.SetDirty(_target);//F2--重命名
        }
    }
    private void TargetReset()
    {
        _target.SetUseCurve(false);
        _target.SetOldTransform();
    }
}
#endif
