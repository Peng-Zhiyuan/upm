using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR 

[CustomEditor(typeof(E3D_GetTransformToShader))]
public class ETAGetTransformToShaderEditor : Editor
{
    private E3D_GetTransformToShader _target;
    private string[] tranformInfoStr = new string[] { "Position", "Rotation", "Scale" };
    private double _previousTime;
    UnityEngine.Object monoScript;

    void OnEnable()
    {
        _target = (E3D_GetTransformToShader)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);

        _previousTime = EditorApplication.timeSinceStartup;
        if (Application.isEditor && Application.isPlaying == false)
        {
            _target.GetNoRunMaterialsList();
        }
        EditorApplication.update += InspectorUpdate;
    }

    void OnDisable()
    {
        EditorApplication.update -= InspectorUpdate;
    }

    private void InspectorUpdate()
    {
        //编辑器下Time的增量需要使用EditorApplication.timeSinceStartup 前后的差值
        var deltaTime = EditorApplication.timeSinceStartup - _previousTime;
        _previousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying)
        {
            _target.TransformInfoChange((float)deltaTime);
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

                _target.infoLists[i].transformTypeIndex = EditorGUILayout.Popup("Transform Type:", _target.infoLists[i].transformTypeIndex, tranformInfoStr);
                _target.infoLists[i].transformType = (E3DGetTransformType)_target.infoLists[i].transformTypeIndex;
                _target.infoLists[i].propertyName = EditorGUILayout.TextField("Shader Property：", _target.infoLists[i].propertyName);
                _target.infoLists[i].applyChild = EditorGUILayout.ToggleLeft("Apply Child：", _target.infoLists[i].applyChild, GUILayout.Width(guiLayoutWidth));

                HoriDeleteBtn<E3DGetTransformToShaderInfo>(ref _target.infoLists, i);
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