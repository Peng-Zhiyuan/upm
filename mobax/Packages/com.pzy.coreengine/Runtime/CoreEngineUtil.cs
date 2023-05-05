using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


public class CoreEngineUtil
{
    public static void InvokCoreDestroy(CoreObject co)
    {
        var list = co.componentList;
        foreach (var comp in list)
        {
            comp.OnCoreDestory();
        }
    }

    public static void CreateDebugerGameObject(CoreObject co)
    {
        var method = AttachMethodInfo();
        method.Invoke(null, new object[] { co });
    }

    static MethodInfo _attachMethod;
    public static MethodInfo AttachMethodInfo()
    {
        if(_attachMethod == null)
        {
            var domain = AppDomain.CurrentDomain;
            var assemblyList = domain.GetAssemblies();
            foreach (var assembly in assemblyList)
            {
                var type = assembly.GetType("CoreObjectDebuger");
                if (type != null)
                {
                    var method = type.GetMethod("Attach", BindingFlags.Public | BindingFlags.Static);
                    _attachMethod = method;
                    break;
                }
            }
        }
        return _attachMethod;
    }

    static Dictionary<string, Type> typeNameToTypeDic = new Dictionary<string, Type>();
    public static Type FindTypeInAllAssembly(string typeName)
    {
        if(typeNameToTypeDic.ContainsKey(typeName))
        {
            return typeNameToTypeDic[typeName];
        }
                 

        var domain = AppDomain.CurrentDomain;
        var assemblyList = domain.GetAssemblies();
        foreach (var assembly in assemblyList)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                typeNameToTypeDic[typeName] = type;
                return type;
            }
        }
        typeNameToTypeDic[typeName] = null;
        return null;
    }

}




