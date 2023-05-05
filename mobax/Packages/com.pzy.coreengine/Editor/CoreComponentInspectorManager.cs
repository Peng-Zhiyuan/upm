using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[InitializeOnLoad]
public static class CoreComponentInspectorManager 
{
    static CoreComponentInspectorManager()
    {
        coreCompTypeToCustomInpectorDic = new Dictionary<Type, Type>();
        var typeList = ReflectionUtil.GetAttributedClassesInAllAssemblies<CustomCoreComponentInspectorAttribute>();
        foreach (var inspectorType in typeList)
        {
            var attribute = inspectorType.GetCustomAttribute<CustomCoreComponentInspectorAttribute>();
            var compType = attribute.coreComponentType;
            coreCompTypeToCustomInpectorDic[compType] = inspectorType;
        }
    }

    public static Type GetInspectorTypeByComponentType(Type coreCompType)
    {
        // 如果有自定义编辑器，返回自定义编辑器类型
        // 否则返回编辑器基类类型
        var dic = coreCompTypeToCustomInpectorDic;
        if (dic.ContainsKey(coreCompType))
        {
            return dic[coreCompType];
        }
        else
        {
            return typeof(CoreComponentInspector);
        }
    }

    


    public static Dictionary<Type, Type> coreCompTypeToCustomInpectorDic;
    //public static Dictionary<Type, Type> CoreCompTypeToCustomInpectorDic
    //{
    //    get
    //    {
    //        if (_coreCompTypeToCustomInpectorDic == null)
    //        {

    //        }
    //        return _coreCompTypeToCustomInpectorDic;
    //    }
    //}

    public static void SetFoldout(string foldKey, bool value)
    {
        var key = $"{typeof(CoreObjectDebugerInspector)}.{foldKey}";
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public static bool IsFoldout(string foldKey)
    {
        var key = $"{typeof(CoreObjectDebugerInspector)}.{foldKey}";
        var intValue = PlayerPrefs.GetInt(key, 1);
        return intValue == 1;
    }

    static Dictionary<Type, MonoScript> compToScriptDic = new Dictionary<Type, MonoScript>();

    public static MonoScript GetScript(Type compType)
    {
       
        if(compToScriptDic.ContainsKey(compType))
        {
            return compToScriptDic[compType];
        }
        else
        {
            var typeName = compType.Name;
            var guidList = AssetDatabase.FindAssets($"{typeName} t:script");
            if (guidList.Length > 0)
            {
                var first = guidList[0];
                var assetPath = AssetDatabase.GUIDToAssetPath(first);
                var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                compToScriptDic[compType] = asset;
                return asset;
            }
            else
            {
                compToScriptDic[compType] = null;
                return null;
            }
        }
    }

}
