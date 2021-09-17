using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;
using System.Reflection;
using UnityEditor.Build.Reporting;

namespace NativeBuilder
{

	public class NativeBuilderUtility
	{

		public static string[] ListBuildScenes()
		{
			List<string> EditorScenes = new List<string>();
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) 
			{
				if (!scene.enabled)
					continue;
				EditorScenes.Add(scene.path);
			}
			return EditorScenes.ToArray();
		}
		
		// EditorUserBuildSettings.androidBuildSystem = buildSystem;
		public static void Build(string target_dir, BuildTarget build_target, BuildOptions build_options)
		{
			string[] scenes = ListBuildScenes ();
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
			var report = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
			var result = report.summary.result;
			if(result == BuildResult.Failed)
			{
				throw new Exception("build pipline returns error");
			}
		}

	}
}
