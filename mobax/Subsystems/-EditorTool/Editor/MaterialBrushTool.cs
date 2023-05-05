using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;
public class MaterialBrushTool
{

	[MenuItem("Assets/CustomBrushTools/EnableGPUInstanceBrush")]
	static void EnableGPUInstanceBrush()
	{
		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(object), SelectionMode.DeepAssets);
		foreach (var activeObject in selection)
		{
			string resPath = AssetDatabase.GetAssetPath(activeObject);
			GameObject go = AssetDatabase.LoadAssetAtPath(resPath, typeof(GameObject)) as GameObject;
			if (go == null)
			{
				continue;
			}
			MeshRenderer meshRender = go.GetOrAddComponent<MeshRenderer>();
			for (int i = 0; i < meshRender.sharedMaterials.Length; i++)
			{
				if (meshRender.sharedMaterials[i] && meshRender.sharedMaterials[i].HasProperty("EnableGPUInstance"))
				{
					meshRender.sharedMaterials[i].SetInt("EnableGPUInstance", 1);
				}
			}
		}
	}
}
