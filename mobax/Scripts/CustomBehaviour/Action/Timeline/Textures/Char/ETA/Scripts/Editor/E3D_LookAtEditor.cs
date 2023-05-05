using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(E3D_LookAt))]
public class E3D_LookAtEditor  : Editor
{
    private E3D_LookAt _target;
    private double _previousTime;
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_LookAt)target;
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

        if (!Application.isPlaying && _target.useCurveMove)
        {
            _target.E3D_RotateLookAtObj((float)deltaTime);
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
        _target.always = EditorGUILayout.Toggle("Always: ", _target.always);
        _target.lockY = EditorGUILayout.Toggle("Lock Y: ", _target.lockY);
        _target.delayTime = EditorGUILayout.FloatField("Delay：", _target.delayTime);
        _target.showLine = EditorGUILayout.Toggle("Show Line: ", _target.showLine);
        _target.alwaysLookAtSpeed = EditorGUILayout.FloatField("Always Speed: ", _target.alwaysLookAtSpeed);
        _target.lookAtObj = (GameObject)EditorGUILayout.ObjectField("LookAt Obj: ", _target.lookAtObj, typeof(GameObject), true);
        _target.lockMovceDir = EditorGUILayout.Toggle("LockMoveDir: ", _target.lockMovceDir);
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

        EditorGUILayout.HelpBox("Tip:\n Default No Looking at direction。 ", MessageType.Warning);

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