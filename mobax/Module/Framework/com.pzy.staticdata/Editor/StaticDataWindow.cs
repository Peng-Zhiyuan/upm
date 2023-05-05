using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.IO;

public class StaticDataWindow : EditorWindow
{
	[MenuItem("pzy.com.*/StaticData/Window")]
	public static void OpenWidnow()
	{
		var window = EditorWindow.GetWindow<StaticDataWindow>("StaticData", true);
		window.Show(true);
	}

	string ExcelPathLocalStorage
    {
		get
        {
			return PlayerPrefs.GetString(nameof(ExcelPathLocalStorage), "");
        }
		set
        {
			PlayerPrefs.SetString(nameof(ExcelPathLocalStorage), value);
		}
    }

	bool ClanCache
	{
		get
		{
			return PlayerPrefs.GetInt(nameof(ClanCache), 0) == 1;
		}
		set
		{
			PlayerPrefs.SetInt(nameof(ClanCache), value ? 1 :0);
		}
	}

	void OnGUI()
    {
		var excelDir = EditorGUILayout.TextField("Excel Dir", ExcelPathLocalStorage);
		ExcelPathLocalStorage = excelDir;

		var cleanCache = EditorGUILayout.Toggle("Clean Cache", ClanCache);
		ClanCache = cleanCache;


		if (GUILayout.Button("Build"))
        {
			this.Build(excelDir, cleanCache);
		}
	}

	void Build(string excelDir, bool cleanCache)
    {
		var findList = Directory.GetFiles(excelDir, "*.xlsx");
		if (findList.Length == 0)
		{
			throw new Exception("[StaticDataWindow] not .xlsx file(s) found");
		}
		var info = new Dictionary<string, string>();
		info["version"] = "temp";
		EtuUtil.Run(excelDir, cleanCache, info);
		Debug.Log("[StaticDataWindow] success");

		File.Copy($"{excelDir}/etu/separatedBufferZip/separatedBuffer.zip", "Assets/EtuGenerate/Resources/embededStaticData.buffer.bytes", true);
	}

}
