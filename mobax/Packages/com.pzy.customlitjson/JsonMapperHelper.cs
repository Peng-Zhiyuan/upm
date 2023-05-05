using System.Collections;
using System.Threading;
namespace CustomLitJson
{
	public class JsonMapperHelper 
	{
		private static JsonMapperHelper _instance;
		public static JsonMapperHelper Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new JsonMapperHelper();
				}
				return _instance;
			}
		}

		static  int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
		public Thread parameterThread;
		// If called in the non main thread, will return false;
		public static bool IsMainThread
		{
			get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
		}

		public   T ToObject<T> (string json)
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
