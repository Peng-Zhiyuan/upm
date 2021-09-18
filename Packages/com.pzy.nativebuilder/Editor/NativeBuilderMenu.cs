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

        [MenuItem("NativeBuilderLight/Build")]
		public static void Build()
		{
			NativeBuilderGUI.Show();
		}

        [MenuItem("NativeBuilderLight/Open Product")]
		public static void OpenAndroidProduct()
		{
			var path = "NativeBuilderProduct";
			Debug.Log(path);
			EditorUtility.OpenWithDefaultApp(path);
		}

	}
}

