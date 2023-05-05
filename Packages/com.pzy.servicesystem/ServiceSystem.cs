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

	Dictionary<ServiceMessage, List<Func<Task>>> asyncMessageToPretaskListDic = new Dictionary<ServiceMessage, List<Func<Task>>>();
	public List<Service> orderedServiceList = new List<Service>();

	public void Setup()
    {
		//var assembly = typeof(DriverManager).Assembly;
		var serviceTypeList = ReflectionUtil.GetSubClassesInAllAssemblies<Service>();

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

	public Service GetService<T>() where T :Service
    {
		var type = typeof(T);
		if(typeToServiceDic.ContainsKey(type))
        {
			return typeToServiceDic[type];
        }
		return null;
    }

	public void RegisterAsyncPretask(ServiceMessage msg, Func<Task> pretask)
    {
		var pretaskList = DictionaryUtil.GetOrCreateList(asyncMessageToPretaskListDic, msg);
		pretaskList.Add(pretask);
	}

	public T GetDriver<T>() where T : Service
    {
		var type = typeof(T);
		var service = typeToServiceDic[type] as T;
		return service;
	}

	public void SendMessage(ServiceMessage msg)
    {
		//foreach(var kv in typeToServiceDic)
  //      {
		//	var driver = kv.Value;
		//	driver.Handle(msg);
  //      }
		foreach(var service in orderedServiceList)
        {
			if(service.enabled)
            {
				service.Handle(msg);
			}
        }
    }

	public async Task SendMessageAsync(ServiceMessage msg)
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
			if(service.enabled)
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

	
}
