using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
public class RefreshEditorBuildSetting
{

	public static void EnableScene(params string[] sceneNameList)
	{
		string BuildSettingPath = Application.dataPath + "/../" + "ProjectSettings/EditorBuildSettings.asset";
		Debug.Log("BuildSettingPath" + BuildSettingPath);
		StreamReader sr = new StreamReader(BuildSettingPath);
		string info = sr.ReadToEnd();
		//Debug.Log("sinfo:" + info);
		sr.Close();

		info = info.Replace("- enabled: 1", "- enabled: 0");

		for (int i = 0; i < sceneNameList.Length; i++)
		{
			string sceneName = sceneNameList[i];
			Debug.Log("key:" + sceneName + ".unity");
			int index = info.IndexOf(sceneName + ".unity");
			if (index > 0)
			{
				Debug.Log("index:" + index);
				var subInfo = info.Substring(0, index);
				int beginIndex = subInfo.LastIndexOf("- enabled:");
				//int endIndex = subInfo.LastIndexOf("  path: ");
				int endIndex = beginIndex + "- enabled: 0".Length;
				var beginInfo = info.Substring(0, beginIndex);
				var endInfo = info.Substring(endIndex);
				Debug.Log("beginIndex:" + index);
				if (beginIndex > 0)
				{

					info = beginInfo + "- enabled: 1" + endInfo;
					Debug.Log("set suc!!!");
				}
			}
		}
		

		StreamWriter sw = new StreamWriter(BuildSettingPath);
		sw.Write(info);
		Debug.Log("tinfo:" + info);
		sw.Close();

		AssetDatabase.Refresh();
	}
	[MenuItem("Tools/Enable Root")]
	public static void EnableRootOnly()
	{
		EnableScene("Root");
	}

	[MenuItem("Tools/Enable RoleRoom And Root")]
	public static void EnableRoleRoomAndRootOnly()
	{
		EnableScene("RoleRoom", "Root");
	}
	[MenuItem("Tools/Enable RoleRoom Only")]
	public static void EnableRoleRoomOnly()
	{
		EnableScene("RoleRoom");
	}
	/*
	[MenuItem("Tools/Enable RoleRoom Only")]
	public static void EnableRoleRoomSceneOnly()
	{
		SetSceneEnable(false);
		EnableScene("RoleRoom");
	}

	[MenuItem("Tools/Enable Root Only")]
	public static void EnableRootSceneOnly()
	{
		SetSceneEnable(false);
		EnableScene("Root");
	}

	[MenuItem("Tools/Enable All Scene")]
	public static void EnableAllScene()
	{
		SetSceneEnable(true);
	}

	[MenuItem("Tools/Disable All Scene")]
	public static void DisableAllScene()
	{
		SetSceneEnable(false);
	}
	*/
	public static void  SetSceneEnable(bool isEnable)
	{
		string BuildSettingPath = Application.dataPath+"/../"+"ProjectSettings/EditorBuildSettings.asset";
		Debug.Log("BuildSettingPath"+BuildSettingPath);
		StreamReader sr = new StreamReader(BuildSettingPath);
		string info = sr.ReadToEnd();
		Debug.Log("sinfo:"+info);
		sr.Close();
		if(isEnable)
		{
			info = info.Replace("- enabled: 0","- enabled: 1");
		}
		else
		{
			info = info.Replace("- enabled: 1","- enabled: 0");
		}

		StreamWriter sw = new StreamWriter(BuildSettingPath);
		sw.Write(info);
		Debug.Log("tinfo:"+info);
		sw.Close();
		Debug.Log("set suc!!!");
		AssetDatabase.Refresh();
	}
	
}