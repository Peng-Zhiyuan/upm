using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class ServiceSystem
{
	static Dictionary<Type, Service> typeToServiceDic = new Dictionary<Type, Service>();

	static Dictionary<string, List<Func<Task>>> asyncMessageToPretaskListDic = new Dictionary<string, List<Func<Task>>>();
	static List<Service> orderedServiceList = new List<Service>();

	public static void Start()
    {
		//var assembly = typeof(DriverManager).Assembly;
		var serviceTypeList = ReflectionUtil.GetSubClassesInAllAssemblies<Service>();

		var count = serviceTypeList.Count;
		Debug.Log($"[ServiceSystem] {count} service(s) found");
		foreach(var serviceType in serviceTypeList)
        {
			var name = serviceType.Name;
			var instance = Activator.CreateInstance(serviceType) as Service;
			instance.OnCreate();
			typeToServiceDic[serviceType] = instance;
			Debug.Log($"[ServiceSystem] {name} created.");
			orderedServiceList.Add(instance);
		}

		// 对 order 进行排序，从小到大
		orderedServiceList.Sort((a, b) =>
		{
			var atype = a.GetType();
			var btype = b.GetType();
			var aattribute = atype.GetCustomAttribute<ServiceOrderAttribute>();
			var battribute = btype.GetCustomAttribute<ServiceOrderAttribute>();
			var aorder = aattribute?.order ?? 0;
			var border = battribute?.order ?? 0;
			return aorder - border;
		});
	}

	public static void RegisterAsyncPretask(string targetMessage, Func<Task> pretask)
    {
		var pretaskList = DictionaryUtil.GetOrCreateList(asyncMessageToPretaskListDic, targetMessage);
		pretaskList.Add(pretask);
	}

	public static T GetDriver<T>() where T : Service
    {
		var type = typeof(T);
		var service = typeToServiceDic[type] as T;
		return service;
	}

	public static void SendMessage(string msg)
    {
		//foreach(var kv in typeToServiceDic)
  //      {
		//	var driver = kv.Value;
		//	driver.Handle(msg);
  //      }
		foreach(var service in orderedServiceList)
        {
			service.Handle(msg);
        }
    }

	public async static Task SendMessageAsync(string msg)
    {
		var stopwatch = new Stopwatch();
		var pretaskList = DictionaryUtil.TryGet(asyncMessageToPretaskListDic, msg, null);
		if(pretaskList != null)
        {
			
			var taskList = new List<Task>();
			foreach(var pretask in pretaskList)
            {
				var task = pretask.Invoke();
				taskList.Add(task);
			}
			stopwatch.Reset();
			stopwatch.Start();
			await Task.WhenAll(taskList);
			stopwatch.Stop();
			var durationMs = stopwatch.ElapsedMilliseconds;
			if (durationMs > 0)
			{
				Debug.Log($"[ServiceSystem] pretask of {msg} cost {durationMs} ms");
			}
		}

		//foreach (var kv in typeToServiceDic)
		//      {
		//	var name = kv.Key;
		//	var driver = kv.Value;
		//	stopwatch.Reset();
		//	stopwatch.Start();
		//	await driver.HandleAsync(msg);
		//	stopwatch.Stop();
		//	var durationMs = stopwatch.ElapsedMilliseconds;
		//	if(durationMs > 0)
		//          {
		//		Debug.Log($"[ServiceSystem] {msg}: {name} cost {durationMs} ms");
		//          }

		//}

		foreach (var service in orderedServiceList)
		{
			var name = service.GetType().Name;
			stopwatch.Reset();
			stopwatch.Start();
			await service.HandleAsync(msg);
			stopwatch.Stop();
			var durationMs = stopwatch.ElapsedMilliseconds;
			if (durationMs > 0)
			{
				Debug.Log($"[ServiceSystem] {msg}: {name} cost {durationMs} ms");
			}

		}
	}

	
}
