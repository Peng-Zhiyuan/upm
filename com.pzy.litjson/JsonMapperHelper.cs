using UnityEngine;
using System.Collections;
using System.Threading;
namespace CustomLitJson
{
	public static class JsonMapperHelper  
	{
		static  int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
		public static bool IsMainThread
		{
			get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
		}

		public static T ToObject<T> (string json)
		{
			if(IsMainThread)
			{
				return JsonMapper.Instance.ToObject<T>(json);
			}
			else
			{
				return JsonThreadMapper.Instance.ToObject<T>(json);
			}
		}
	}
}
