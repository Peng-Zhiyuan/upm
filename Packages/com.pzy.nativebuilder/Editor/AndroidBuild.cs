using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using NativeBuilder;
//using XLua;



public static class AndroidBuild
{
	static DateTime startTime;

	//build Eclipse Project
	public static void Build(bool developmen = false)
	{

		startTime = DateTime.Now;

		// pre build
		NativeBuilderPluginManager.NotifyPreBuild();

		AssetDatabase.Refresh();

		string solutionPath = "NativeBuilderProduct/eclipse_project";

		//check parent dir
		{
			var di = new DirectoryInfo(solutionPath).Parent;
			if(!di.Exists){
				di.Create();
			}
		}
		
		//if target Path exits, delete it!
		if (Directory.Exists (solutionPath)) {
			DirectoryInfo dir = new DirectoryInfo (solutionPath);
			dir.Delete (true);
		}


		// build eclipse project
		UnityEngine.Debug.Log("Unity building eclipse project..."); 
		if(developmen){
			NativeBuilderUtility.Build(solutionPath, UnityEditor.BuildTarget.Android, UnityEditor.BuildOptions.Development);
		}
		else{
			NativeBuilderUtility.Build(solutionPath, UnityEditor.BuildTarget.Android, UnityEditor.BuildOptions.None);
		}
				
		// rename main android module to Game
		string oldGameName = PlayerSettings.productName;
		string newGamename = "Game";
		string source = Path.Combine(solutionPath, oldGameName);
		string dest = Path.Combine(solutionPath, newGamename);
		Directory.Move(source, dest);

		// rename obb
		string ObbPath = solutionPath+"/"+PlayerSettings.productName+".main.obb";
		if(File.Exists(ObbPath))
		{
			string targetObbPath = solutionPath + "/"+"main."+PlayerSettings.Android.bundleVersionCode+"."+PlayerSettings.applicationIdentifier+".obb";
			File.Move(ObbPath, targetObbPath);
		}

		// add android append file
		AddAndroidAppendFile(solutionPath + "/Game");

		// pre build
		NativeBuilderPluginManager.NotifyPostBuild();

		// 检查是否有失败标记
		NativeBuilderPluginManager.SureNotMarkedFail();

		DateTime endTime = DateTime.Now;
		var useTime = endTime - startTime;
		string msg = "[NativeBuilder] Build Success";
		msg += ", android project release at [" + "NativeBuilderProduct/eclipse_project/Game]";
		msg += ", in "  + FormatTime(useTime);
		Debug.Log(msg);
	}
	
	private static void AddAndroidAppendFile(string outputFolder)
	{
		var list = Directory.GetDirectories(Application.dataPath, "AndroidAppend", SearchOption.AllDirectories);
		if(list.Length == 0)
		{
			return;
		}
		var androidAppend = list[0];
		PShellUtil.CopyTo(androidAppend, outputFolder, PShellUtil.FileExsitsOption.Override, PShellUtil.DirectoryExsitsOption.Override, new string[]{".meta"});
	}

	private static string FormatTime(TimeSpan span)
	{
		return string.Format("{0:F0} munutes {1:F0} seconds" ,span.TotalMinutes, span.Seconds);
	}
	
}


