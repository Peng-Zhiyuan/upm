using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;

public class NativeBuilderPluginManager
{
    static List<NativeBuilderPlugin> _pluginList;
	static List<NativeBuilderPlugin> PluginList
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

	static bool isMarkedFail = false;

	public static void NotifyPreBuild()
    {
		isNotifiedFirstSceneProcessed = false;
		isMarkedFail = false;

		foreach (var plugin in PluginList)
        {
			var typeName = plugin.GetType().Name;
			Debug.Log($"NativeBuilder: {typeName} OnPreBuild");
			plugin.OnPreBuild();
		}
    }

	public static void SureNotMarkedFail()
    {
		if(isMarkedFail)
        {
			throw new Exception("[NativeBuilderPluginManager] marked fail. search Exception to find error");
        }
    }

	public static void NotifyPostBuild()
	{
		foreach (var plugin in PluginList)
		{
			var typeName = plugin.GetType().Name;
			Debug.Log($"NativeBuilder: {typeName} OnPostBuild");
			plugin.OnPostBuild();
		}
	}

	public static void NotifyFirstSceneProccesed()
	{
		foreach (var plugin in PluginList)
		{
			var typeName = plugin.GetType().Name;
			Debug.Log($"NativeBuilder: {typeName} OnFirstSceneProccessed");
			plugin.OnFirstSceneProccessed();
		}
	}

	public static List<NativeBuilderPlugin> CreateAllPluginInstance()
    {
		var subClazzList = GetSubClassesInAllLoadedAssembly<NativeBuilderPlugin>();
		var ret = new List<NativeBuilderPlugin>();
		foreach (var type in subClazzList)
		{
			var instance = Activator.CreateInstance(type) as NativeBuilderPlugin;
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

	static bool isNotifiedFirstSceneProcessed = false;
	/// <summary>
	/// 当处理完一个场景，
	/// 这时目标平台的程序集已经生成
	/// </summary>
	[PostProcessScene]
	public static void PostProcessScene()
	{

		if (isNotifiedFirstSceneProcessed)
		{
			return;
		}
		isNotifiedFirstSceneProcessed = true;

		try
        {
			NotifyFirstSceneProccesed();
		}
		catch
        {
			isMarkedFail = true;
			throw;
		}
	}
}
