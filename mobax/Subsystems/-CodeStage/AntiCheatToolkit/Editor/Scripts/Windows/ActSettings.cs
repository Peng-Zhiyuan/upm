using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Windows
{
	internal class ActSettings: EditorWindow
	{
		private const string WIREFRAME_SHADER_NAME = "Hidden/ACTk/WallHackTexture";

		private static SerializedObject graphicsSettingsAsset;
		private static SerializedProperty includedShaders;

		[UnityEditor.MenuItem(ActEditorGlobalStuff.WINDOWS_MENU_PATH + "Settings...")]
		internal static void ShowWindow()
		{
			ActSettings myself = GetWindow<ActSettings>(false, "ACTk Settings", true);
			myself.minSize = new Vector2(500, 250);
		}

		private void OnGUI()
		{
			GUILayout.Label("Injection Detector settings (global)", ActEditorGUI.LargeBoldLabel);

			bool enableInjectionDetector = EditorPrefs.GetBool(ActEditorGlobalStuff.PREFS_INJECTION_ENABLED);

			EditorGUI.BeginChangeCheck();
			enableInjectionDetector = GUILayout.Toggle(enableInjectionDetector, "Enable Injection Detector");
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(ActEditorGlobalStuff.PREFS_INJECTION_ENABLED, enableInjectionDetector);
				if (enableInjectionDetector && !ActPostprocessor.IsInjectionDetectorTargetCompatible())
				{
					Debug.LogWarning(ActEditorGlobalStuff.LOG_PREFIX + "Injection Detector is not available on selected platform (" + EditorUserBuildSettings.activeBuildTarget + ")");
				}

				if (!enableInjectionDetector)
				{
					ActEditorGlobalStuff.CleanInjectionDetectorData();
				}
				else if (!File.Exists(ActEditorGlobalStuff.injectionDataPath))
				{
					ActPostprocessor.InjectionAssembliesScan();
				}
			}

			if (GUILayout.Button("Edit Whitelist"))
			{
				ActAssembliesWhitelist.ShowWindow();
			}
			
			EditorGUILayout.Space();
			GUILayout.Label("WallHack Detector settings (per-project)", ActEditorGUI.LargeBoldLabel);
			GUILayout.Label("Wireframe module uses specific shader under the hood. Thus such shader should be included into the build to exist at runtime. To make sure it's get included, you may add it to the Always Included Shaders list using buttons below. You don't need to include it if you're not going to use Wireframe module.", EditorStyles.wordWrappedLabel);

			ReadGraphicsAsset();

			if (graphicsSettingsAsset != null && includedShaders != null)
			{
				// outputs whole included shaders list, use for debug
				//EditorGUILayout.PropertyField(includedShaders, true);

				int shaderIndex = GetWallhackDetectorShaderIndex();

				EditorGUI.BeginChangeCheck();

				if (shaderIndex != -1)
				{
					GUILayout.Label("Shader already exists in the Always Included Shaders list, you're good to go!", EditorStyles.wordWrappedLabel);
					if (GUILayout.Button("Remove shader"))
					{
						includedShaders.DeleteArrayElementAtIndex(shaderIndex);
						includedShaders.DeleteArrayElementAtIndex(shaderIndex);
                    }
				}
				else
				{
					GUILayout.Label("Shader doesn't exists in the Always Included Shaders list.", EditorStyles.wordWrappedLabel);
					if (GUILayout.Button("Include shader"))
					{
						Shader shader = Shader.Find(WIREFRAME_SHADER_NAME);
						if (shader != null)
						{
							includedShaders.InsertArrayElementAtIndex(includedShaders.arraySize);
							SerializedProperty newItem = includedShaders.GetArrayElementAtIndex(includedShaders.arraySize - 1);
							newItem.objectReferenceValue = shader;
						}
						else
						{
							Debug.LogError("Can't find " + WIREFRAME_SHADER_NAME + " shader! Please report this to the  " + ActEditorGlobalStuff.REPORT_EMAIL + " including your Unity version number.");
						}
					}
					if (GUILayout.Button("Open Graphics Settings to manage it manually (see readme.pdf for details)"))
					{
						EditorApplication.ExecuteMenuItem("Edit/Project Settings/Graphics");
					}
				}

				if (EditorGUI.EndChangeCheck())
				{
					graphicsSettingsAsset.ApplyModifiedProperties();
				}
			}
			else
			{
				GUILayout.Label("Can't automatically control " + WIREFRAME_SHADER_NAME + " shader existence at the Always Included Shaders list. Please, manage this manually in Graphics Settings.");
				if (GUILayout.Button("Open Graphics Settings"))
				{
					EditorApplication.ExecuteMenuItem("Edit/Project Settings/Graphics");
				}
			}
		}

		internal static void ReadGraphicsAsset()
		{
			if (graphicsSettingsAsset != null) return;

			Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
			if (assets.Length > 0)
			{
				graphicsSettingsAsset = new SerializedObject(assets[0]);
			}

			if (graphicsSettingsAsset != null)
			{
				includedShaders = graphicsSettingsAsset.FindProperty("m_AlwaysIncludedShaders");
			}
		}

		internal static int GetWallhackDetectorShaderIndex()
		{
			if (graphicsSettingsAsset == null || includedShaders == null) return -1;

			int result = -1;
			graphicsSettingsAsset.Update();

			int itemsCount = includedShaders.arraySize;
			for (int i = 0; i < itemsCount; i++)
			{
				SerializedProperty arrayItem = includedShaders.GetArrayElementAtIndex(i);
				if (arrayItem.objectReferenceValue != null)
				{
					Shader shader = (Shader)(arrayItem.objectReferenceValue);

					if (shader.name == WIREFRAME_SHADER_NAME)
					{
						result = i;
						break;
					}
				}
			}

			return result;
		}

		internal static bool IsWallhackDetectorShaderIncluded()
		{
			bool result = false;

			ReadGraphicsAsset();
			if (GetWallhackDetectorShaderIndex() != -1)
				result = true;

			return result;
		}
	}
}