using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ServiceSystem : StuffObject<ServiceSystem>
{
	Dictionary<Type, Service> typeToServiceDic = new Dictionary<Type, Service>();

	public List<Service> serviceList = new List<Service>();

	/// <summary>
	/// 从程序集创建服务对象
	/// </summary>
	/// <param name="assembly">null 表示搜索所有已加载的程序集, 否则只搜索特定程序集</param>
	public void Create(Assembly assembly = null, string excludeAssembleNameInAllSerachMode = null)
    {
		Debug.Log($"[ServiceSystem] start to create services");
		//var assembly = typeof(DriverManager).Assembly;
		List<Type> serviceTypeList = null;
		if (assembly == null)
        {
			serviceTypeList = ReflectionUtil.GetSubClassesInAllAssemblies<Service>(excludeAssembleNameInAllSerachMode);
		}
		else
        {
			serviceTypeList = ReflectionUtil.GetSubClasses<Service>(assembly);
		}

		var count = serviceTypeList.Count;
		Debug.Log($"[ServiceSystem] {count} service(s) found");
		foreach(var serviceType in serviceTypeList)
        {
			var name = serviceType.Name;
			//if (name == "JsService" || name == "XLuaService") continue;
			//Debug.LogError("serviceType:"+ name);
			var instance = Activator.CreateInstance(serviceType) as Service;
			instance.OnCreate();
			typeToServiceDic[serviceType] = instance;
			Debug.Log($"[ServiceSystem] {name} created.");
			serviceList.Add(instance);
		}

	}

	public Service GetService<T>() where T :Service
    {
		var type = typeof(T);
		if(typeToServiceDic.ContainsKey(type))
        {
			return typeToServiceDic[type];
        }
		return null;
    }
	
}
