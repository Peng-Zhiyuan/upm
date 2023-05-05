using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public static class CoroutineManager
{

	static CoroutineProvider _provider;
    static CoroutineProvider provider
	{
		get
		{
			if(_provider == null)
			{
				var go = new GameObject("CoroutineProvider");
				_provider = go.AddComponent<CoroutineProvider>();
                GameObject.DontDestroyOnLoad(go);
			}
			return _provider;
		}
	}

    static Dictionary<string, IEnumerator> dic = new Dictionary<string, IEnumerator>();

    public static void Create(IEnumerator enumerator)
    {
        provider.StartCoroutine(enumerator);
    }

    public static void Create(string id, IEnumerator enumerator)
    {
        Stop(id);
        dic[id] = enumerator;
        provider.StartCoroutine(enumerator);
    }

    public static void Stop(string id)
    {
        IEnumerator e;
        dic.TryGetValue(id, out e);
        if (e != null)
        {
            provider.StopCoroutine(e);
        }
        dic.Remove(id);
    }

}

public class CoroutineProvider : MonoBehaviour
{

}
