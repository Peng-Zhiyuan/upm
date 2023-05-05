using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class ThreadMgr:Single<ThreadMgr>{
	Dictionary<string,Thread> mThreadDic=new Dictionary<string, Thread>();
	public bool disableThread = false;

	public void Start(string tag,Thread thread, object  param = null)
	{
		thread.IsBackground = true;
		End(tag);
		if(param==null)
		{
			thread.Start();
		}
		else
		{
			thread.Start(param);
		}
		mThreadDic.Add(tag,thread);
	}
	public void EndAll()
	{
		foreach(KeyValuePair<string,Thread> _kv in mThreadDic)
		{
			if(_kv.Value!=null)
			{
				_kv.Value.Join();
			}
		}
		mThreadDic.Clear();
	}
	void OnDestroy()
	{
		EndAll();
	}
	public void End(string tag)
	{
		if(mThreadDic.ContainsKey(tag))
		{
			if(mThreadDic[tag]!=null)
			{
				mThreadDic[tag].Join();
				mThreadDic[tag]=null;
				mThreadDic.Remove(tag);
			}
		}
	}
}
