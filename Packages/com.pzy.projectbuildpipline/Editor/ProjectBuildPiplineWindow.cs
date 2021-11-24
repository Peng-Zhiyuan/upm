using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ProjectBuildPiplineWindow : EditorWindow
{
	[MenuItem("ProjectBuildPipline/Window")]
	public new static void Show()
	{
		var window = EditorWindow.GetWindow<ProjectBuildPiplineWindow>("Project Build Pipline", true) as ProjectBuildPiplineWindow;
		window.Show(true);
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
			EditorGUILayout.HelpBox(msg, MessageType.Info);

			var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			var disabled = buildTarget != currentBuildTarget;
			if(disabled)
            {
				EditorGUI.BeginDisabledGroup(true);
			}
			if (GUILayout.Button(name))
			{
				ProjectBuildPipline.Build(name);
			}
			if (disabled)
			{
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}