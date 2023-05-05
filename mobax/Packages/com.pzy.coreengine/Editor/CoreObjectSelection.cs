using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class CoreObjectSelection 
{
	static public CoreTransform SelectedActiveCoreTransform
	{
		get
		{
			var monoTransform = Selection.activeTransform;
			var debuger = monoTransform.GetComponent<CoreObjectDebuger>();
			return debuger?.GetComponent<CoreTransform>();

		}
	}

	static public CoreTransform[] SelectedCoreTransform
	{
		get
		{
			var list = new List<CoreTransform>();
			var monoTransform = Selection.transforms;
			foreach (var one in monoTransform)
			{
				var debuger = one.GetComponent<CoreObjectDebuger>();
				var coreTransform = debuger?.GetComponent<CoreTransform>();
				list.Add(coreTransform);
			}
			return list.ToArray();



		}
	}
}
