using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

public class BundleBuilderPluginManager
{
    static List<BundleBuilderPlugin> _pluginList;
	static List<BundleBuilderPlugin> PluginList
    {
		get
        {
			if(_pluginList == null)
            {
				var list = CreateAllPluginInstance();
				_pluginList = list;
			}
			return _pluginList;
		}
    }


	public static void NotifyPreBuild()
    {
		foreach(var plugin in PluginList)
        {
			var typeName = plugin.GetType().Name;
			Debug.Log($"BundleBuilder: {typeName} OnPreBuild");
			plugin.OnPreBuild();
		}
    }

	public static void NotifyPostBuild()
	{
		foreach (var plugin in PluginList)
		{
			var typeName = plugin.GetType().Name;
			Debug.Log($"BundleBuilder: {typeName} OnPostBuild");
			plugin.OnPostBuild();
		}
	}

	public static List<BundleBuilderPlugin> CreateAllPluginInstance()
    {
		var subClazzList = GetSubClassesInAllLoadedAssembly<BundleBuilderPlugin>();
		var ret = new List<BundleBuilderPlugin>();
		foreach (var type in subClazzList)
		{
			var instance = Activator.CreateInstance(type) as BundleBuilderPlugin;
			ret.Add(instance);
		}
		return ret;
	}

	private static List<Type> GetSubClasses<T>(Assembly assembly)
	{
		var subTypeQuery = from t in assembly.GetTypes()
						   where (typeof(T).IsAssignableFrom(t) && typeof(T) != t)
						   select t;
		return subTypeQuery.ToList();
	}

	public static List<Type> GetSubClassesInAllLoadedAssembly<T>() 
	{
		var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
		var ret = new List<Type>();
		foreach(var assembly in assemblyList)
        {
			var list = GetSubClasses<T>(assembly);
			ret.AddRange(list);
		}
		return ret;
	}
}
