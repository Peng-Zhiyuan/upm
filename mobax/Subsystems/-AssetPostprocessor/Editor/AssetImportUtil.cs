using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
public class AssetImportUtil 
{
	public static bool Custom_Tex_M(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.EndsWith("_m") || fileName.Contains("_m_");
	}

	public static bool Custom_Tex_Splat(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.StartsWith("splat");
	}
	public static bool Custom_Tex_lightMap(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.Contains("lightmap");
	}

	public static bool Custom_Tex_UI(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.EndsWith("_ui");
	}
	public static bool Custom_Tex_DirLightMap(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.Contains("_dir");
	}

	public static bool Custom_Tex_CubeMap(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.Contains("reflectionprobe");
	}

	public static bool Custom_Tex_Fx(string path)
	{
		return path.Contains("FX/");
	}

	public static bool Custom_Tex_MipMap(string path)
	{
		return !(path.Contains("FX/") || path.Contains("Spine/") ||path.Contains("BattleFont/"));
	}

	public static bool Custom_Tex_Normal(string path)
	{
		string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
		return fileName.EndsWith("_n") || fileName.Contains("_n_") || fileName.Contains("_normal");
	}
	public static bool TryPostprocessFx(string path)
	{
		var fxRoot = $"Assets/Arts/FX";
		if (!path.Contains(fxRoot))
		{
			return false;
		}
		var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
		if (prefab == null)
		{
			return false;
		}
		var psList = prefab.GetComponentsInChildren<ParticleSystem>();
		if (psList == null
		    || psList.Length == 0)
		{
			return false;
		}
		bool isChange = false;
		var comp = prefab.GetComponent<Fx>();
		if (comp == null)
		{
			prefab.AddComponent<Fx>();
			isChange = true;
		}
		if (path.Contains($"Assets/Arts/FX/fx_chararcter")
		    || path.Contains($"Assets/Arts/FX/fx_monster"))
		{
			for (int i = 0; i < psList.Length; i++)
			{
				ParticleSystem.MainModule main = psList[i].main;
				if (main.maxParticles > 100)
				{
					main.maxParticles = 100;
					isChange = true;
				}
				if (main.prewarm)
				{
					main.prewarm = false;
					isChange = true;
				}
			}
		}
		
		if (isChange)
		{
			Debug.Log("Fix Fx parameter " + path);
		}
		return isChange;
	}

	/// <summary>
	/// ��Ŀ¼������ prefab ������
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path">Assets ��ͷ��·��</param>
	public static void AddComponentToAllPrefabInFolder<T>(string path) where T : Component
	{
		var prefabList = AssetDatabaseUtil.LoadAllAssetInFolder<GameObject>(path);
		foreach (var prefab in prefabList)
		{
			if (prefab.GetComponent<T>() == null)
			{
				prefab.AddComponent<T>();
				EditorUtility.SetDirty(prefab);
			}
		}
		AssetDatabase.SaveAssets();
	}

	private static Avatar GetSourceAvatar(string sourceAvatarPrefabPath)
	{
		UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(sourceAvatarPrefabPath, typeof(GameObject));
		if (prefab == null)
		{
			return null;
		}
		GameObject sourceAvatarObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;

		var sourceAvatarAnimator = sourceAvatarObject.GetComponentInChildren<Animator>();

		var sourceAvatar = sourceAvatarAnimator.avatar;
		GameObject.DestroyImmediate(sourceAvatarObject);
		return sourceAvatar;
	}
	public static async void TrySetSourceAvatar(ModelImporter modelImporter, string path)
	{
		var avatar = GetSourceAvatar(path);
		if (avatar == null)
		{
			Debug.Log("sourceAvatar is null, retry!");
			await Task.Delay(100);
			avatar = GetSourceAvatar(path);

		}
		Debug.Log("sourceAvatar set success!!!");
		if(avatar != null) modelImporter.sourceAvatar = avatar;
	}

	public static async void TrySetSourceMask(ModelImporterClipAnimation modelImporterClipAnim, string path)
	{
		var mask = GetSourceMask(path);
		if (mask == null)
		{
			Debug.Log("sourceMask is null, retry!");
			await Task.Delay(100);
			mask = GetSourceMask(path);

		}
		Debug.Log("sourceMask set success!!!");
		if(mask != null) modelImporterClipAnim .maskSource= mask;
	}

	private static AvatarMask GetSourceMask(string sourceMaskPath)
	{
		Debug.Log("sourceMaskPath:"+ sourceMaskPath);
		AvatarMask avatarMask = AssetDatabase.LoadAssetAtPath(sourceMaskPath, typeof(AvatarMask)) as AvatarMask;
		return avatarMask;
	}
	public static  async void ImportAvatarMaskDelay(AssetImporter importer)
	{
		await Task.Delay(100);
		
		AvatarMask avatarMask = AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(AvatarMask)) as AvatarMask;
		if (avatarMask != null)
		{
			Debug.Log("assetPath:" + importer.assetPath);
			for (int i = 0; i < avatarMask.transformCount; i++)
			{
				string path = avatarMask.GetTransformPath(i);
				Debug.Log("path:"+path);
				if (path.StartsWith("wp_") || path.StartsWith("Bip001") || path.Contains("_weapon_"))
				{
					avatarMask.SetTransformActive(i, true);
				}
				else 
				{
					avatarMask.SetTransformActive(i, false);
				}
			}
			

		}
	}
}
