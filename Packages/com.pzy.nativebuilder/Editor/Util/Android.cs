using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Diagnostics;

namespace NativeBuilder
{

	public enum AntMod
	{
		Debug,
		Release,
	}


	public class Android  {

		public static bool CheckIsAndroidSDK(string path)
		{

			if (!Directory.Exists (path))
			{
				return false;
			}

			// check folder exists
			DirectoryInfo di = new DirectoryInfo(path);
			if(!di.Exists) return false;

			// check adb
			var ADB = OSUtil.Platform == Platform.Mac ? "adb" : "adb.exe";
			if(di.GetFiles(ADB, SearchOption.AllDirectories).Length == 0) return false;

			return true;
		}

		public static void RunAPK(string apkPath, string package, string androidSdkPath)
		{
			// find adb
			var ADB = OSUtil.Platform == Platform.Mac ? "adb" : "adb.exe";
			var files = new DirectoryInfo(androidSdkPath).GetFiles(ADB, SearchOption.AllDirectories);
			if(files.Length == 0){
				throw new IOException("adb not found in '" + androidSdkPath + "'!");
			}
			FileInfo adb = files[0];
			// adb device
			if(!HasDevices(androidSdkPath)){
				throw new Exception("No Device Connected, Running task canceled");
			}
			// adb install
			string s = Exec.RunGetOutput(adb.FullName, "install -r " + apkPath, true);
			if(s.Contains("Failure")){
				throw new Exception("Error in adb install");
			}
			// adb monkey
			var code2 = Exec.Run(adb.FullName, "shell monkey -p " + package  + " -v 1");
			if(code2 != 0){
				throw new Exception("Error in adb monkey");
			}
		}

		public static bool HasDevices(string androidSdkPath)
		{
			// find adb
			var ADB = OSUtil.Platform == Platform.Mac ? "adb" : "adb.exe";
			var files = new DirectoryInfo(androidSdkPath).GetFiles(ADB, SearchOption.AllDirectories);
			if(files.Length == 0){
				throw new IOException("adb not found in '" + androidSdkPath + "'!");
			}
			FileInfo adb = files[0];

			// adb device
			string s = Exec.RunGetOutput(adb.FullName, " devices");
			s = s.Substring(s.IndexOf("List of devices attached"));
			s = s.Substring(24);
			UnityEngine.Debug.Log("get string from 'devices': " + s);
			if(s == null || !s.Contains("device") || s.Contains("error") || s.Contains("waiting for device") || s.Contains("*")){
				return false;
			}
			return true;
		}

		public static void android_update(string projectPath, string android_sdk_path)
		{


			FileInfo android;
			{
				var di = new DirectoryInfo(android_sdk_path);
				if(!di.Exists)
				{
					throw new IOException("android sdk not exsists!");
				}
				var ANDROID = OSUtil.Platform == Platform.Mac ? "android" : "android.bat";
				var files = di.GetFiles(ANDROID, SearchOption.AllDirectories);
				if(files.Length == 0)
				{
					throw new IOException("None '" + ANDROID + "' was found in adnroid sdk");
				}
				else if(files.Length > 1)
				{
					UnityEngine.Debug.Log("Mutiple '" + ANDROID + "' was found, use this one: " + files[0].FullName);
				}
				android = files[0];
			}
			// find largest target id
			string maxTargetName = GetMaxTargetName(android_sdk_path);
			var code = Exec.Run(android.FullName, "update project -p " + projectPath, true);
			//var code = Exec.Run(android.FullName, "update project -p " + projectPath + " -t " + maxTargetName, true);
			//var code = Exec.RunEx(android.FullName, true, "update", "project", "-p", projectPath, "-t", maxTarget);
			if(code != 0){
				throw new IOException("Error in android update");
			}
		}

		public static string GetMaxTargetName(string android_sdk_path)
		{
			var output = android_list_target(android_sdk_path);
			
			string maxTargetName = null;
			for(int i = 9 ; i < 30 ; i ++)
			{
				string toFind = "android-" + i;
				if(output.IndexOf(toFind) != -1){
					maxTargetName = toFind;
					continue;
				}
				toFind = "Google Inc.:Google APIs:" + i;
				if(output.IndexOf(toFind) != -1){
					maxTargetName = toFind;
					continue;
				}
			}
			return maxTargetName;
		}

		public static int GetMaxTargetLevel(string android_sdk_path)
		{
			var output = android_list_target(android_sdk_path);
			
			int maxTargetLevel = 0;
			for(int i = 9 ; i < 30 ; i ++)
			{
				string toFind = "android-" + i;
				if(output.IndexOf(toFind) != -1){
					maxTargetLevel = i;
					continue;
				}
				toFind = "Google Inc.:Google APIs:" + i;
				if(output.IndexOf(toFind) != -1){
					maxTargetLevel = i;
					continue;
				}
			}
			return maxTargetLevel;
		}

		public static string android_list_target(string android_sdk_path)
		{
			FileInfo android;
			{
				var di = new DirectoryInfo(android_sdk_path);
				if(!di.Exists)
				{
					throw new IOException("android sdk not exsists!");
				}
				var ANDROID = OSUtil.Platform == Platform.Mac ? "android" : "android.bat";
				var files = di.GetFiles(ANDROID, SearchOption.AllDirectories);
				if(files.Length == 0)
				{
					throw new IOException("None '" + ANDROID + "' was found in adnroid sdk");
				}
				else if(files.Length > 1)
				{
					UnityEngine.Debug.Log("Mutiple '" + ANDROID + "' was found, use this one: " + files[0].FullName);
				}
				android = files[0];
			}

			var output = Exec.RunGetOutput(android.FullName, "list target", true);
			return output;
		}


		
		public static FileInfo FindAntBuiltAPK(string projectPath, AntMod antMod){
			var pattern = antMod == AntMod.Debug ? "*-debug.apk" : "*-release.apk";
			
			DirectoryInfo di = new DirectoryInfo(projectPath + "/bin");
			var files = di.GetFiles(pattern);
			if(files.Length == 0){
				throw new IOException("not found '" + pattern + "' in '" + projectPath + "'");
			}
			return files[0];
		}
		
		public static void MakeSrcDir(string projectPath)
		{
			DirectoryInfo di = new DirectoryInfo(projectPath + "/src");
			if(!di.Exists)
			{
				di.Create();
			}
		}


	}

}

