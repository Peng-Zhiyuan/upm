using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO; 
public class AheadAssetCollect
{
	private static bool FilterBundleName(string path, List<string> keys)
	{
		foreach (string key in keys)
		{
			if (path.Contains(key))
			{
				return true;
			}
		}
		return false;
	}

	private static  bool FilterParentPath(string path, List<string> keys)
	{
		foreach (string key in keys)
		{
			if (path.StartsWith(key))
			{
				return true;
			}
		}
		return false;
	}

	private static bool FilterFileType(string path, List<string> keys)
	{
		foreach (string key in keys)
		{
			
			
			if (path.Contains(key))
			{
				
				return true;
			}
		}
		return false;
	}

	private static bool CheckParentPath(string path, List<string> keys)
	{
		foreach (string key in keys)
		{
			if (path.StartsWith(key))
			{
				return true;
			}
		}
		return false;
	}
	[MenuItem("Tools/CollectAheadResLog")]
	static void CollectAheadResLog()
	{
		CollectAheadRes(false);
	}

	[MenuItem("Tools/CollectAheadRes")]
	static void CollectAheadRes()
	{
		CollectAheadRes(true);
	}

	
	static void CollectAheadRes(bool execute)
	{
		string assetPath = "Assets/AddressableAssetsExtData/used.txt";
		TextAsset textAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset)) as TextAsset;
		string text = textAsset.text;
		var resArray= text.Split('\n');
		
		Debug.LogError("resList:"+ resArray.Length);
		List<string> filterBundleName = new List<string>() { "$client_config_res","$guide","$BattleFont","$pkg_ahead", "$Font" };

		List<string> filterFileType = new List<string>() { ".cs",".dll" };

		//List<string> filterParentPath= new List<string>() { "Packages", "Assets/UISprites/atlas", "Assets/LegacyAtlas" , "Assets/Subsystems/-NGGrass", "Assets/Wwise", "Assets/wwiseData", "Assets/Arts/Models" };
		List<string> filterParentPath = new List<string>() { "Assets/Arts/Env/EnvCommon/env_street" };
		List<string> selectParentPath = new List<string>() { "Assets/Arts/Env/EnvCommon" };
		List<string> selectBundleName = new List<string>() { "$Data", "$skybox", "$ui_bg", "$plot_bg", "$ui_bg_loading", "$ui_bubble_head", "$ui_heroAvatar", "$ui_heroBook", "$ui_heroCards", "$ui_stage", "$ui_atlas", "$Plot3D","$UI"};
		List<string> resList = new List<string>();
		foreach(string path in resArray)
		{
			if (string.IsNullOrEmpty(path)) continue;
			if (path.Contains("_ahead"))
			{
				Debug.Log("ahead skip:" + path);
				continue;
			}
			if (FilterFileType(path, filterFileType)) continue;
			//if (FilterBundleName(path, filterBundleName)) continue;
			if (FilterParentPath(path, filterParentPath)) continue;
			if (FilterParentPath(path, selectParentPath)|| FilterBundleName(path, selectBundleName))
			{
				resList.Add(path.Trim());
			}
		}

		foreach (string path in resList)
		{
			
			string assetName = Path.GetFileName(path);

			if (!path.Contains("$"))
			{
					int nIndex = path.LastIndexOf("/");
					string newDir = path.Substring(0, nIndex)+ "/$pkg_ahead";
					string newPath = $"{newDir}/{assetName}";
					if (!Directory.Exists(newDir))
					{
						if (execute)
						{
							Directory.CreateDirectory(newDir);
						}
					}

					if (!File.Exists(path))
					{
						Debug.Log("skip:" + path + " -> " + $"{newPath}");
						continue;
					}
					if (execute)
					{
						
						var result = AssetDatabase.MoveAsset(path, newPath);
						if (string.IsNullOrEmpty(result)) Debug.Log(path + " -> " + $"{newPath}");
						else Debug.LogError(path + " -> " + $"{newPath} result:" + result);
					}
					else 
					{
						Debug.Log(path + " -> " + $"{newPath}");
					}
            }
            else 
			{
				int tempIndex = path.LastIndexOf("$");
				int index = path.IndexOf("/", tempIndex);
				if (index < 0)
				{
					Debug.LogError("skip path:" + path);
					continue;
				}
				var tempPath = path.Insert(index, "_ahead");
				string newDir = Path.GetDirectoryName(tempPath);
				string newPath = $"{newDir}/{assetName}";
				if (!Directory.Exists(newDir))
				{
					if (execute)
					{
						Directory.CreateDirectory(newDir);
					}
				}
				if (!File.Exists(path))
				{
					Debug.Log("skip:" + path + " -> " + $"{newPath}");
					continue;
				}
				if (execute)
				{
					
					var result = AssetDatabase.MoveAsset(path, newPath);
					if(string.IsNullOrEmpty(result)) Debug.Log("$:"+path + " -> " + $"{newPath}");
					else Debug.LogError(path + " -> " + $"{newPath} result:" + result);
				}
				else
				{
					Debug.Log("$:" + path + " -> " + $"{newPath}");
				}

			}
			
		}
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

	}
}
