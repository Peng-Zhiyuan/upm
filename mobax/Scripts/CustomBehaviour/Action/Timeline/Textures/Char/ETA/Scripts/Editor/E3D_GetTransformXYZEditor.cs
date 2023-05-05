using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR 
[CustomEditor(typeof(E3D_GetTransformXYZ))]
public class E3D_GetTransformXYZEditor : Editor
{
    private E3D_GetTransformXYZ _target;
    private string[] positionInfoStr = new string[] { "X", "Y", "Z" };
    private double _previousTime;
    UnityEngine.Object monoScript;
    bool _isPlay = false;

    void OnEnable()
    {
        _target = (E3D_GetTransformXYZ)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);
        if (!Application.isPlaying)
            _target.SaveOldTransform();
        _previousTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += InspectorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= InspectorUpdate;

        if (!Application.isPlaying && target)
        {
            _target.SetOldTransform();
        }
    }

    private void InspectorUpdate()
    {
        //编辑器下Time的增量需要使用EditorApplication.timeSinceStartup 前后的差值
        var deltaTime = EditorApplication.timeSinceStartup - _previousTime;
        _previousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying && _isPlay)
        {
            //_target.TransformInfoChange((float)deltaTime);
            _target.TransformInfoChangeExtension((float)deltaTime);//2020-11-09
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

        TransformInfoEditorShow(ref _target.infoLists, DrawTransformInfoShow);

        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Play", GUILayout.MinWidth(100)))
        {
            _isPlay = true;
            _target.SetOldTransform();
        }

        if (GUILayout.Button("Reset", GUILayout.MinWidth(100)))
        {
            _isPlay = false;
            _target.SetOldTransform();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(_isPlay);
        if (GUILayout.Button("IC", GUILayout.MinWidth(100)))
        {
            _target.SetCacheTransformInfo();//2020-11-09
            _target.SaveOldTransform();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Help", GUILayout.MinWidth(100)))
        {
            Application.OpenURL("https://www.element3ds.com/thread-216743-1-1.html");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.HelpBox("Tip:\n Material property Transfrom", MessageType.Warning);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_target);//F2--重命名
        }
    }
    private void TransformInfoEditorShow<T>(ref List<T> list, TransformInfoShow transformInfoShow)
    {
        EditorGUILayout.BeginVertical("box");

        transformInfoShow();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create "))
        {
            T t = default(T);
            AddListItem<T>(ref list, t);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);

        EditorGUILayout.EndVertical();
    }

    private int guiLayoutWidth = 100;
    private void DrawTransformInfoShow()
    {
        if (_target.infoLists.Count > 0)
        {
            for (int i = 0; i < _target.infoLists.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                _target.infoLists[i].gameObject = (GameObject)EditorGUILayout.ObjectField("GameObject: ", _target.infoLists[i].gameObject, typeof(GameObject), true);
                EditorGUILayout.LabelField("Info Area：", GUILayout.Width(guiLayoutWidth));
                _target.infoLists[i].positionTypeIndex = EditorGUILayout.Popup("Position Type:", _target.infoLists[i].positionTypeIndex, positionInfoStr);
                _target.infoLists[i].positionType = (E3D_Position_Axis)_target.infoLists[i].positionTypeIndex;

                HoriDeleteBtn<E3DGetTransformXYZInfo>(ref _target.infoLists, i);
                EditorGUILayout.EndVertical();
            }
        }
    }

    private delegate void TransformInfoShow();
    /// <summary>
    /// 添加一个List对象
    /// </summary>
    private void AddListItem<T>(ref List<T> list, T temp)
    {
        list.Add(temp);
    }
    /// <summary>
    /// 删除按钮
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tempList"></param>
    /// <param name="index"></param>
    private void HoriDeleteBtn<T>(ref List<T> tempList, int index)
    {
        if (GUILayout.Button("Delect"))
        {
            RemoveGradientItem<T>(ref tempList, index);
        }
    }
    /// <summary>
    /// 移除一个List对象
    /// </summary>
    /// <param name="index"></param>
    private void RemoveGradientItem<T>(ref List<T> list, int index)
    {
        if (list.Count > 0 && list.Count > index)
        {
            list.RemoveAt(index);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif