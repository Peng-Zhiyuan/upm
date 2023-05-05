using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
[CustomEditor(typeof(E3D_Material_Animation))]
public class E3D_Material_AnimationEditor : Editor
{
    private E3D_Material_Animation _target;
    private UnityEngine.Object monoScript;
    private double _previousTime;

    private SerializedProperty customColorGradients;
    private string[] shaderCtrStr = new string[] { "Slider", "Float", "Texture_UV", "Color" };
    GUIContent delete = new GUIContent("Delete");
    Color idleBackColor;
    private const float deletBtnWidth = 100;
    private const float curveHeight = 60;
    private const float curveWidth = 160;

    void OnEnable()
    {
        _target = (E3D_Material_Animation)target;
        monoScript = MonoScript.FromMonoBehaviour(this.target as MonoBehaviour);

        if (Application.isEditor && Application.isPlaying == false)
        {
            _target.SetUseCurve(false);
            _target.executeUpdating = new MaterialUpdatingPropertyValue(_target.transform, true);
            _target.CacheDefaultMaterialsProperty(true);
        }
        //=================================================================================//
        idleBackColor = GUI.backgroundColor;
        _previousTime = EditorApplication.timeSinceStartup;
        EditorApplication.update += InspectorUpdate;
        //=================================================================================//
    }


    void OnDisable()
    {
        EditorApplication.update -= InspectorUpdate;
        if (!Application.isPlaying && _target.useCurveMove)
        {
            if (_target)
            {
                _target.ResetAllMaterials();
            }
        }
        //对象停止播放时恢复默认
        if (!Application.isPlaying && _target._isPlay && _target.useCurveMove == false)
        {
            if (_target)
            {
                _target.ResetAllMaterials();
            }
        }
    }


    private void InspectorUpdate()
    {
        //编辑器下Time的增量需要使用EditorApplication.timeSinceStartup 前后的差值
        var deltaTime = EditorApplication.timeSinceStartup - _previousTime;
        _previousTime = EditorApplication.timeSinceStartup;

        if (!Application.isPlaying && _target.useCurveMove)
        {
            _target.ShaderPropertyChange((float)deltaTime);
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

        customColorGradients = serializedObject.FindProperty("colorGradients");
        EditorGUILayout.BeginVertical(GUI.skin.box);

        _target.times = EditorGUILayout.FloatField("Times：", _target.times);
        _target.loop = EditorGUILayout.Toggle("Loop：", _target.loop);
        _target.delayTime = EditorGUILayout.FloatField("Delay：", _target.delayTime);

        EditorGUILayout.LabelField("Curve：");
        _target.curve = EditorGUILayout.CurveField("", _target.curve, GUILayout.Height(60), GUILayout.MinWidth(100));

        VFXCtrType_Mat();

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
            if (Application.isEditor && Application.isPlaying == false)
            {
                _target.CacheDefaultMaterialsProperty(true);
            }
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Help", GUILayout.MinWidth(100)))
        {
            Application.OpenURL("https://www.element3ds.com/thread-216743-1-1.html");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.HelpBox("Tip:\n Property/Tiling_X    Property/Tiling_Y \n Property/Offset_X    Property/Offset_Y", MessageType.Warning);

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
            _target.ResetAllMaterials();
        }
    }
    /// <summary>
    /// 材质类型
    /// </summary>
    private void VFXCtrType_Mat()
    {
        EditorGUILayout.BeginVertical();
        _target.eShaderPropertyTypeIndex = EditorGUILayout.Popup("MatCtrlType:", _target.eShaderPropertyTypeIndex, shaderCtrStr);
        _target.eShaderPropertyType = (EShaderPropertyType)_target.eShaderPropertyTypeIndex;

        switch (_target.eShaderPropertyType)
        {
            case EShaderPropertyType.Range:
                ShaderPropertyEditorShow(ref _target.rangeLists, "Slider", DrawCustomRangeDataList);
                break;
            case EShaderPropertyType.Float:
                ShaderPropertyEditorShow(ref _target.floatList, "Float", DrawCustomFloatDataList);
                break;
            case EShaderPropertyType.TexEnv:
                ShaderPropertyEditorShow(ref _target.textureSTList, "Texture_ST", DrawCustomTexSTDataList);
                break;
            case EShaderPropertyType.Color:
                ShaderPropertyEditorShow(ref _target.colorGradients, "Color", DrawCustomGradientDataList);
                break;
            default:
                break;
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// shader 属性控制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="btnLable"></param>
    /// <param name="shaderProperTyShow"></param>
    private void ShaderPropertyEditorShow<T>(ref List<T> list, string btnLable, ShaderProperTyShow shaderProperTyShow)
    {
        EditorGUILayout.BeginVertical("box");

        shaderProperTyShow();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Creat " + btnLable))
        {
            T t = default(T);
            AddListItem<T>(ref list, t);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(4);

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 添加一个List对象
    /// </summary>
    private void AddListItem<T>(ref List<T> list, T temp)
    {
        list.Add(temp);
    }

    /// <summary>
    /// 提示信息显示
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    private void ShowMessageInfo(string message, MessageType messageType)
    {
        EditorGUILayout.HelpBox(message, messageType);
    }

    /// <summary>
    /// 绘制贴图Tiling / Offset 控制属性
    /// </summary>
    private void DrawCustomTexSTDataList()
    {
        for (int i = 0; i < _target.textureSTList.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);

            string propertyName = _target.textureSTList[i].propertyName;

            _target.textureSTList[i].propertyName = EditorGUILayout.TextField("Property_ST：", _target.textureSTList[i].propertyName);

            if (propertyName != null)
            {
                string[] tempStr = _target.textureSTList[i].propertyName.Split('/');
                if(tempStr.Length == 2)
                {
                    _target.textureSTList[i].targetPropertyName = tempStr[0];
                    //=========================================================//
                    string[] tempStrStr = tempStr[1].Split('_');
                    if (tempStrStr.Length == 2)
                    {
                        _target.textureSTList[i].specificStr = tempStrStr[0];
                        _target.textureSTList[i].axisName = tempStrStr[1];
                    }
                }
            }
            _target.textureSTList[i].floatResult = EditorGUILayout.FloatField("Value：", _target.textureSTList[i].floatResult);
            _target.textureSTList[i].scaleCurve = EditorGUILayout.FloatField("Value Scale：", _target.textureSTList[i].scaleCurve);

            _target.textureSTList[i].applyChild = EditorGUILayout.ToggleLeft("Apply Child：", _target.textureSTList[i].applyChild, GUILayout.Width(guiLayoutWidth));
            _target.textureSTList[i].useSelfAnimCurve = EditorGUILayout.ToggleLeft("Only Curve：", _target.textureSTList[i].useSelfAnimCurve, GUILayout.Width(guiLayoutWidth));
            if (_target.textureSTList[i].useSelfAnimCurve == true)
            {
                _target.textureSTList[i].animCurve = EditorGUILayout.CurveField("", _target.textureSTList[i].animCurve, GUILayout.Height(curveHeight - 8), GUILayout.MinWidth(guiLayoutWidth));
            }
            HoriDeleteBtn<CurveAnimTexSTShaderPro>(ref _target.textureSTList, i);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 绘制float 属性
    /// </summary>
    private void DrawCustomFloatDataList()
    {
        for (int i = 0; i < _target.floatList.Count; i++)
        {

            EditorGUILayout.BeginVertical(GUI.skin.box);
            _target.floatList[i].propertyName = EditorGUILayout.TextField("Property_Float：", _target.floatList[i].propertyName);

            _target.floatList[i].floatResult = EditorGUILayout.FloatField("Value：", _target.floatList[i].floatResult);
            _target.floatList[i].scaleCurve = EditorGUILayout.FloatField("Value Scale：", _target.floatList[i].scaleCurve);

            _target.floatList[i].applyChild = EditorGUILayout.ToggleLeft("Apply Child：", _target.floatList[i].applyChild, GUILayout.Width(guiLayoutWidth));
            _target.floatList[i].useSelfAnimCurve = EditorGUILayout.ToggleLeft("Only Curve：", _target.floatList[i].useSelfAnimCurve, GUILayout.Width(guiLayoutWidth));
            if (_target.floatList[i].useSelfAnimCurve == true)
            {
                _target.floatList[i].animCurve = EditorGUILayout.CurveField("", _target.floatList[i].animCurve, GUILayout.Height(curveHeight - 8), GUILayout.MinWidth(guiLayoutWidth));
            }

            HoriDeleteBtn<CurveAnimFloatShaderPro>(ref _target.floatList, i);

            EditorGUILayout.EndVertical();
        }

    }

    private int guiLayoutWidth = 100;
    /// <summary>
    /// 绘制Range属性
    /// </summary>
    private void DrawCustomRangeDataList()
    {
        if (_target.rangeLists.Count > 0)
        {
            for (int i = 0; i < _target.rangeLists.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                _target.rangeLists[i].propertyName = EditorGUILayout.TextField("Property_Range：", _target.rangeLists[i].propertyName);

                EditorGUILayout.LabelField("Range Area：", GUILayout.Width(guiLayoutWidth));

                EditorGUILayout.BeginHorizontal();
                _target.rangeLists[i].rangeInputMin = EditorGUILayout.FloatField("", _target.rangeLists[i].rangeInputMin, GUILayout.Width(guiLayoutWidth));
                _target.rangeLists[i].rangeInputMax = EditorGUILayout.FloatField("", _target.rangeLists[i].rangeInputMax, GUILayout.Width(guiLayoutWidth));
                EditorGUILayout.EndHorizontal();

                _target.rangeLists[i].rangeResult = EditorGUILayout.FloatField("Value：", _target.rangeLists[i].rangeResult);

                _target.rangeLists[i].applyChild = EditorGUILayout.ToggleLeft("Apply Child：", _target.rangeLists[i].applyChild, GUILayout.Width(guiLayoutWidth));
                _target.rangeLists[i].useSelfAnimCurve = EditorGUILayout.ToggleLeft("Only Curve：", _target.rangeLists[i].useSelfAnimCurve, GUILayout.Width(guiLayoutWidth));
                if (_target.rangeLists[i].useSelfAnimCurve == true)
                {
                    _target.rangeLists[i].animCurve = EditorGUILayout.CurveField("", _target.rangeLists[i].animCurve, GUILayout.Height(curveHeight - 8), GUILayout.MinWidth(guiLayoutWidth));
                }
                HoriDeleteBtn<CurveAnimRangeShaderPro>(ref _target.rangeLists, i);
                EditorGUILayout.EndVertical();
            }
        }
    }

    /// <summary>
    /// 绘制渐变色
    /// </summary>
    private void DrawCustomGradientDataList()
    {
        if (customColorGradients.arraySize <= 0) return;

        EditorGUILayout.BeginVertical();
        for (int i = 0; i < customColorGradients.arraySize; i++)
        {
            SerializedProperty myGradientData = customColorGradients.GetArrayElementAtIndex(i);
            DrawMyGradientData(myGradientData, i);
            HorizontalLine(GUI.skin, Color.gray);
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制细线
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="color"></param>
    public static void HorizontalLine(GUISkin skin, Color color, RectOffset rectOffset = null)
    {
        GUIStyle splitter = new GUIStyle(skin.box);
        splitter.border = new RectOffset(1, 1, 1, 1);
        splitter.stretchWidth = true;
        if (rectOffset == null)
            splitter.margin = new RectOffset(3, 3, 7, 7);
        else
            splitter.margin = rectOffset;

        Color restoreColor = GUI.contentColor;
        GUI.contentColor = color;
        GUILayout.Box("", splitter, GUILayout.Height(1.0f));

        GUI.contentColor = restoreColor;
    }

    /// <summary>
    /// 绘制渐变色具体属性
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    private void DrawMyGradientData(SerializedProperty item, int index)
    {
        EditorGUILayout.BeginVertical();

        if (_target.colorGradients.Count > 0 && _target.colorGradients.Count > index)
        {
            _target.colorGradients[index].propertyName = EditorGUILayout.TextField("Property_Color：", _target.colorGradients[index].propertyName);
        }

        SerializedProperty gradient = item.FindPropertyRelative("colorGradient");
        EditorGUILayout.PropertyField(gradient, new GUIContent("GradientColor："));

        SerializedProperty applyChild = item.FindPropertyRelative("applyChild");
        EditorGUILayout.PropertyField(applyChild, new GUIContent("Apply Child："));

        HoriDeleteBtn<CurveAnimColorShaderPro>(ref _target.colorGradients, index);
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
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

    private delegate void ShaderProperTyShow();

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