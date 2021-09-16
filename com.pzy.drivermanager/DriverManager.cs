using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class DriverManager
{
	static Dictionary<Type, Driver> typeToServiceDic = new Dictionary<Type, Driver>();

    public static void Boost()
    {
		//var assembly = typeof(DriverManager).Assembly;
		var serviceTypeList = ReflectionUtil.GetSubClassesInAllAssemblies<Driver>();

		var count = serviceTypeList.Count;
		Debug.Log($"[DriverManager] {count} driver(s) found");
		foreach(var serviceType in serviceTypeList)
        {
			var name = serviceType.Name;
			var instance = Activator.CreateInstance(serviceType) as Driver;
			instance.Handle("boost");
			typeToServiceDic[serviceType] = instance;
			Debug.Log($"[DriverManager] {name} created.");
		}
	}


	public static T GetDriver<T>() where T : Driver
    {
		var type = typeof(T);
		var service = typeToServiceDic[type] as T;
		return service;
	}

	public static void SendMessage(string msg)
    {
		foreach(var kv in typeToServiceDic)
        {
			var driver = kv.Value;
			driver.Handle(msg);
        }
    }
}
