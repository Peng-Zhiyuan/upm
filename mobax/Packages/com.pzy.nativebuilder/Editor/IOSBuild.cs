using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using NativeBuilder;

public class IOSBuild 
{
	private static void AddIOSAppendFile(string outputFolder)
	{
		var list = Directory.GetDirectories(Application.dataPath, "IOSAppend", SearchOption.AllDirectories);
		if(list.Length == 0)
		{
			return;
		}
		var androidAppend = list[0];
		PShellUtil.CopyTo(androidAppend, outputFolder, PShellUtil.FileExsitsOption.Override, PShellUtil.DirectoryExsitsOption.Override, new string[]{".meta"});
	}

	public static string Build ()
	{

		// pre build
		NativeBuilderPluginManager.NotifyPreBuild();

		AssetDatabase.Refresh();

		// set a target xCode path 
		//var xCodePath = "NativeBuilderProduct/xCode_project";
		var xCodePath = "iOS/xCode_project";

		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
		{
			throw new Exception("Current platform must be iOS! (now is " + EditorUserBuildSettings.activeBuildTarget + ")");
		}

		Debug.Log("Unity building xCode..."); 

		// check parent dir exsits
		DirectoryInfo directory =  new DirectoryInfo(xCodePath);
		var parent = directory.Parent;
		if(!parent.Exists){
			parent.Create();
		}
		//if(directory.Exists)
		//{
		//	directory.Delete(true);
		//}

		// build
		NativeBuilderUtility.Build(xCodePath, UnityEditor.BuildTarget.iOS, UnityEditor.BuildOptions.AcceptExternalModificationsToPlayer);


		// addpend file
		AddIOSAppendFile(xCodePath);

		// pre build
		NativeBuilderPluginManager.NotifyPostBuild();

		// 确保没有任何失败标记
		NativeBuilderPluginManager.SureNotMarkedFail();

		Debug.Log("complete");
		return xCodePath;
	}
}


