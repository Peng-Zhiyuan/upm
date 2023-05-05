using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(CoreObjectDebuger))]
public class CoreObjectDebugerInspector : Editor
{
    CoreObject _co;
    public CoreObject Co
    {
        get
        {
            if(_co == null)
            {
                var coDebuger = target as CoreObjectDebuger;
                var co = coDebuger.co;
                this._co = co;
            }
            return _co;
        }
    }


    void GuiLine(int i_height, Color c)
    {
        //(0.05f, 0.05f, 0.05f, 1)
        Rect rect = EditorGUILayout.GetControlRect(false, i_height);

        rect.height = i_height;
        rect.x -= 18;
        rect.width += 22;

        EditorGUI.DrawRect(rect, c);

    }

    private void OnSceneGUI()
    {
        var componentList = this.Co.componentList;
        foreach (var comp in componentList)
        {
            CoreComponentInspector coreComponentInspector;
            this.compToInpectorDic.TryGetValue(comp, out coreComponentInspector);
            coreComponentInspector.OnSceneGUI();

        }
    }


    public override void OnInspectorGUI()
    {
        var componentList = this.Co.componentList;

        bool first = true;
        foreach(var comp in componentList)
        {
            if(first)
            {
                first = false;
            }
            else
            {
                GuiLine(1, new Color(0.05f, 0.05f, 0.05f, 1));
            }

           // GUILayout.BeginVertical("GroupBox");
            //EditorGUI.indentLevel++;
            DrawComponent(comp);
            //EditorGUI.indentLevel--;
            //GUILayout.EndVertical();

            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        }
    }


    Dictionary<CoreComponent, CoreComponentInspector> compToInpectorDic = new Dictionary<CoreComponent, CoreComponentInspector>();

    private void OnEnable()
    {
        // 为每一个组件创建编辑器对象
        var compList = this.Co.componentList;
        foreach (var comp in compList)
        {
            var type = comp.GetType();
            var inspectorType = CoreComponentInspectorManager.GetInspectorTypeByComponentType(type);
            var inspector = Activator.CreateInstance(inspectorType) as CoreComponentInspector;
            inspector.debuggerUnityInspector = this;
            inspector.target = comp;
            inspector.OnEnable();
            compToInpectorDic[comp] = inspector;
        }
    }

    private void OnDisable()
    {
        foreach(var kv in compToInpectorDic)
        {
            var inspector = kv.Value;
            inspector.OnDisable();
        }
    }




    void DrawComponent(CoreComponent comp)
    {
        // 绘制绘制组件
        CoreComponentInspector coreComponentInspector;
        this.compToInpectorDic.TryGetValue(comp, out coreComponentInspector);
        var inspectorType = coreComponentInspector.GetType();

        // 绘制头
        GUILayout.BeginHorizontal();

        var type = comp.GetType();
        var typeName = type.Name;

        // fold
        var isFoldout = CoreComponentInspectorManager.IsFoldout(typeName);

        var displayName = typeName;
        if(inspectorType != typeof(CoreComponentInspector))
        {
            displayName += " (" + inspectorType.Name + ")";
        }
        //var postIsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldout, displayName, null, (rect) => ShowHeaderContextMenu(comp, rect));
        var postIsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(isFoldout, displayName);
        if (postIsFoldout != isFoldout)
        {
            isFoldout = postIsFoldout;
            CoreComponentInspectorManager.SetFoldout(typeName, isFoldout);
        }


        EditorGUILayout.EndFoldoutHeaderGroup();
        GUILayout.EndHorizontal();


        if (isFoldout)
        {
            GuiLine(1, new Color(0.175f, 0.175f, 0.175f, 1));
            if (coreComponentInspector == null)
            {
                EditorGUILayout.HelpBox("Inspector Not Found", MessageType.Error);
            }
            else
            {
                EditorGUILayout.Separator();
                coreComponentInspector.OnInspectorGUI();
                EditorGUILayout.Separator();
            }

        }
        //var isEnabled = comp.IsEnabled;
        //comp.IsEnabled = EditorGUILayout.ToggleLeft(typeName, isEnabled);

        



     
    }

    //void ShowHeaderContextMenu(CoreComponent comp, Rect position)
    //{
    //    var menu = new GenericMenu();
    //    menu.AddItem(new GUIContent("Edit"), false, () => OnEditClicked(comp));
    //    menu.DropDown(position);
    //}

    //void OnEditClicked(CoreComponent comp)
    //{
    //    var compName = comp.GetType().Name;
    //    var guidList = AssetDatabase.FindAssets($"{compName} t:script");
    //    if(guidList.Length >0)
    //    {
    //        var first = guidList[0];
    //        var assetPath = AssetDatabase.GUIDToAssetPath(first);
    //        Application.OpenURL(assetPath);
    //    }
    //    else
    //    {
    //        Debug.LogError("not founed script file named: " + compName);
    //    }
    //}
}
