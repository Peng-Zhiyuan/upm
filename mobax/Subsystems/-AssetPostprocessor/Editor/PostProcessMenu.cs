using UnityEngine;
using UnityEditor;
using System.Collections;

public class PostProcessMenu
{
	public static bool IsAssetPostProcessEnable
	{
		get
		{

			return PlayerPrefs.GetInt("IsAssetPostProcessEnable",0) ==1;
		}
	}
	[MenuItem("Postprocess/Enable AssetPostProcessor")]
	public static void AssetPostProcessEnable()
	{
		PlayerPrefs.SetInt ("IsAssetPostProcessEnable", 1);
		PlayerPrefs.Save ();
	}

	[MenuItem("Postprocess/Disable AssetPostProcessor")]
	public static void AssetPostProcessDisable()
	{
		PlayerPrefs.SetInt ("IsAssetPostProcessEnable", 0);
		PlayerPrefs.Save ();
	}

	[MenuItem("Postprocess/Enable AssetPostProcessor", true)]
	static bool AssetPostProcessEnableCheck()
	{
		return !IsAssetPostProcessEnable;
	}

	[MenuItem("Postprocess/Disable AssetPostProcessor", true)]
	static bool AssetPostProcessDisableCheck()
	{
		return IsAssetPostProcessEnable;
	}
}

