using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ProjectBuildPiplineWindow : EditorWindow
{
	[MenuItem("pzy.com.*/ProjectBuildPipline/Window")]
	public new static void Show()
	{
		var window = EditorWindow.GetWindow<ProjectBuildPiplineWindow>("Project Build Pipline", true) as ProjectBuildPiplineWindow;
		window.Show(true);
	}

	Dictionary<string, Dictionary<string, string>> schemaToParamDicDic = new Dictionary<string, Dictionary<string, string>>();
	string GetParam(string schema, string key, string defaultValue)
    {
		var paramDic = DictionaryUtil.GetOrCreateDic(schemaToParamDicDic, schema);
		var value = DictionaryUtil.TryGet(paramDic, key, defaultValue);
		return value;
    }

	void SetParam(string schema, string key, string value)
    {
		var paramDic = DictionaryUtil.GetOrCreateDic(schemaToParamDicDic, schema);
		paramDic[key] = value;
	}

	Dictionary<string, string> GetParamDic(string schema)
    {
		var dic = DictionaryUtil.GetOrCreateDic(schemaToParamDicDic, schema);
		return dic;
	}

	public void OnGUI()
	{
		EditorGUILayout.LabelField("Build Schemas:");
		var schemaTypeList = ProjectBuildPipline.SchemaTypeList;
		foreach (var type in schemaTypeList)
		{
			var name = type.Name;
			var attribute = type.GetCustomAttribute<BuildSchemaAttribute>();
			string msg = "";
			BuildTarget buildTarget = BuildTarget.NoTarget;
			if(attribute != null)
            {
				msg = attribute.msg;
				buildTarget = attribute.buildTarget;
            }

			// 帮助信息
			if(msg != "")
            {
				EditorGUILayout.HelpBox(msg, MessageType.Info);
			}

			// 参数
			var attrList = ProjectBuildPipline.GetSchemaParamList(type.Name);
			foreach(var attr in attrList)
            {
				var key = attr.key;
				var defaultValue = attr.defaultValue;
				var value = GetParam(name, key, defaultValue);

				var isDiffrentFromDefault = value != defaultValue;

				if(isDiffrentFromDefault)
                {
					ChnageColor(); 
				}
				
				var postValue = EditorGUILayout.TextField(key, value);

				if (isDiffrentFromDefault)
				{
					ResetColor();
				}

				if (postValue != value)
                {
					SetParam(name, key, postValue);
                }
			}

			var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			var disabled = buildTarget != BuildTarget.NoTarget && buildTarget != currentBuildTarget;
			if(disabled)
            {
				EditorGUI.BeginDisabledGroup(true);
			}
			if (GUILayout.Button(name))
			{
				var paramDic = GetParamDic(name);
				ProjectBuildPipline.Build(name, paramDic);
			}
			if (disabled)
			{
				EditorGUI.EndDisabledGroup();
			}
		}
	}

	static Color old;
	static void ChnageColor()
    {
		old = GUI.color;
		GUI.color = new Color(0, 1f, 0.5f);
	}

	static void ResetColor()
    {
		GUI.color = old;
	}
}