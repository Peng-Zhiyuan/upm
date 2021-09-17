using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NativeBuilder
{
	public class NativeBuilderGUI : EditorWindow 
	{
		
		public new static void Show()
		{
			var window = EditorWindow.GetWindow<NativeBuilderGUI>("Build", true) as NativeBuilderGUI;
			window.Show(true);
		}

		public void OnGUI()
		{
			// platform
			{
				var buildTarget = EditorUserBuildSettings.activeBuildTarget;
				GUI.enabled = false;
				EditorGUILayout.TextField("Build Target", buildTarget.ToString());
				GUI.enabled = true;
			}
			
			// player name
			{
				var productName = PlayerSettings.productName;
				productName = EditorGUILayout.TextField("Product Name", productName);
				PlayerSettings.productName = productName;
			}

			// package name
			{
				var a = PlayerSettings.applicationIdentifier;
				var b = EditorGUILayout.TextField("Identifier", a);
				//PlayerSettings.applicationIdentifier = b;
				PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, b);
				PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, b);
			}

			// version
			{
				var version = PlayerSettings.bundleVersion;
				version = EditorGUILayout.TextField("Version", version);
				PlayerSettings.bundleVersion = version;
			}

			// android version code
			{
				var code = PlayerSettings.Android.bundleVersionCode;
				code = int.Parse(EditorGUILayout.TextField("Android Version Code", code.ToString()));
				PlayerSettings.Android.bundleVersionCode = code;
			}

			// build system
			{
				var isExportAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
				var post = EditorGUILayout.Toggle("ExportAndroidProject", isExportAndroidProject);
				EditorUserBuildSettings.exportAsGoogleAndroidProject = post;
			}
			

			// build scenes
			{
				var list = EditorBuildSettings.scenes;
				foreach (var s in list)
				{
					var enable = GUILayout.Toggle(s.enabled, s.path);
					s.enabled = enable;
				}
				EditorBuildSettings.scenes = list;
			}

			if(GUILayout.Button("Build"))
			{
				var buildTarget = EditorUserBuildSettings.activeBuildTarget;
				if(buildTarget == BuildTarget.Android)
				{
					AndroidBuild.Build();
				}
				else if(buildTarget == BuildTarget.iOS)
				{
					IOSBuild.Build();
				}
				else if(buildTarget == BuildTarget.WebGL)
				{
					WebGLBuild.Build();
				}
			}

			if(GUILayout.Button("Open ProductDirectory"))
			{
				OpenProductDir();
			}

		}


		public void OpenProductDir()
		{
			var path = "NativeBuilderProduct";
			Debug.Log(path);
			EditorUtility.OpenWithDefaultApp(path);
		}
	

	}

}

