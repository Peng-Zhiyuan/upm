using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using CustomLitJson;
using System.IO;
using UnityEditor;
using UnityEditor;

public class UnityAutoInitEditor 
{

    [MenuItem("HotSystem/GenUnityAutoInitFile")]
    public static void GenerateHotAssemblyAutoInitList()
    {
        var path = $"Assets/$assembly/RuntimeInitializeOnLoads.json";
        var infoList = GetAutoInitInfoList();
        var json = JsonMapper.Instance.ToJson(infoList);
        json = JsonUtil.Buitify(json);
        var d = Path.GetDirectoryName(path);
        if(!Directory.Exists(d))
        {
            Directory.CreateDirectory(d);
        }
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    static List<AutoInitInfo> GetAutoInitInfoList()
    {
        var methodList = GetTargetMethodList();
        var infoList = new List<AutoInitInfo>();
        foreach (var one in methodList)
        {
            var info = new AutoInitInfo();
            var type = one.DeclaringType;
            info.assemblyName = type.Assembly.GetName().Name;
            info.nameSpace = type.Namespace;
            info.className = type.Name;
            info.methodName = one.Name;
            infoList.Add(info);
        }
        return infoList;
    }


    static List<MethodInfo> GetTargetMethodList()
    {
        var retMethodList = new List<MethodInfo>();
        var hotAssembly = "Assembly-CSharp";
        var assembly = ReflectionUtil.GetAssembly(hotAssembly);
        var typeList = assembly.GetTypes();
        foreach (var type in typeList)
        {
            var methodList = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methodList)
            {
                var attribute = method.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>();
                if (attribute != null)
                {
                    retMethodList.Add(method);
                }
            }
        }
        return retMethodList;
    }

}
