using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace NativeBuilder 
{
	public class NativeBuilderMenu 
	{

        [MenuItem("pzy.com.*/NativeBuilderLight/Build")]
		public static void Build()
		{
			NativeBuilderGUI.Show();
		}

        [MenuItem("pzy.com.*/NativeBuilderLight/Open Product")]
		public static void OpenAndroidProduct()
		{
			var path = "NativeBuilderProduct";
			Debug.Log(path);
			EditorUtility.OpenWithDefaultApp(path);
		}

	}
}

