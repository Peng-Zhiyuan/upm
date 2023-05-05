using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NG_SyncMashRenderBrush
{

/*	[MenuItem("Assets/CustomBrushTools/SyncMashRenderBrush")]
	static void SyncMashRenderBrush()
	{
		
		string path = EditorUtility.OpenFilePanel("请选择模板文件", Application.dataPath+"/Arts/Models/Role", "prefab");
		int index = path.IndexOf("Assets/");
		string copyFromPath = path.Substring(index);
		Debug.Log("copyFromPath:" + copyFromPath);
		foreach (var o in Selection.objects)
		{
			var copyToPath = AssetDatabase.GetAssetPath(o);
			Debug.Log("copyToPath:" + copyToPath);
			DoCopyComponent(copyFromPath, copyToPath);
		}
	}*/

	[MenuItem("Assets/CustomBrushTools/SyncMashRenderBrush")]
	static void SyncMashRenderBrush()
	{
		if (Selection.objects.Length != 2) return;
		var copyFromPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
		var copyToPath = AssetDatabase.GetAssetPath(Selection.objects[1]);
		string msg = "FromPrefab:" + copyFromPath + "\nToPrefab:" + copyToPath;
		
		bool ok = EditorUtility.DisplayDialog("拷贝材质球",msg,"ok");
		if (ok)
		{
			DoCopyComponent(copyFromPath, copyToPath);
		}

	}

	private static string ConvertKey(string key)
	{
		if (char.IsNumber(key[0]))
		{
			int index = key.IndexOf("_");
			key = key.Substring(index + 1);
		}
		return key;
	}
	private static void DoCopyComponent(string copyFrom, string copyTo)
	{
		if (copyFrom == null || copyTo == null)
		{
			Debug.LogError("路径为null copyFrom：" + copyFrom + "   copyTo:"+ copyTo);
			return;
		}
		Debug.Log("路径copyFrom：" + copyFrom + "   copyTo:" + copyTo);
		var copyFromGo = PrefabUtility.LoadPrefabContents(copyFrom) as GameObject;
		var copyToGo = PrefabUtility.LoadPrefabContents(copyTo) as GameObject;
		if (copyToGo.GetComponent<Animator>() != null)
		{
			copyToGo.GetOrAddComponent<RoleRender>();
		}

		var copyFromComponents = copyFromGo.GetComponentsInChildren<Renderer>();
		var copyToComponents = copyToGo.GetComponentsInChildren<Renderer>();
		var dic = new Dictionary<string, Renderer>();
		foreach (Renderer c in copyFromComponents)
		{
			var key = ConvertKey(c.gameObject.name);
			//Debug.LogError("copyfrom节点名称:" + key);
			dic[key] = c;
		}

		foreach (Renderer c in copyToComponents)
		{
			var key = ConvertKey(c.gameObject.name);
			//Debug.LogError("copyto节点名称:" + key);
			if (!dic.ContainsKey(key))
			{
				Debug.LogError("节点名称不匹配:"+key);
				continue;
			}
			c.sharedMaterials = dic[key].sharedMaterials;
			Debug.LogError("mats赋值suc:" + key);
			/*
			for (int i = 0; i < dic[key].sharedMaterials.Length; i++)
			{
				if (dic[key].sharedMaterials[i] == null)
				{
					Debug.LogError("mat is null:" + i);
					continue;
				}
				Debug.LogError("mat 赋值:"+ dic[key].sharedMaterials[i].name);
				c.sharedMaterials[i] = dic[key].sharedMaterials[i];
			}
			*/
		}
		PrefabUtility.SaveAsPrefabAssetAndConnect(copyToGo, copyTo, InteractionMode.AutomatedAction);
		GameObject.DestroyImmediate(copyToGo);
		GameObject.DestroyImmediate(copyFromGo);
	}


}
