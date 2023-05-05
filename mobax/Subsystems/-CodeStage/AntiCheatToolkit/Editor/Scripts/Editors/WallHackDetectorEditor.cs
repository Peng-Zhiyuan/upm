using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.EditorCode.Windows;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	[CustomEditor(typeof (WallHackDetector))]
	internal class WallHackDetectorEditor : ActDetectorEditor
	{
		private SerializedProperty wireframeDelay;
		private SerializedProperty raycastDelay;
		private SerializedProperty spawnPosition;
		private SerializedProperty maxFalsePositives;

		private SerializedProperty checkRigidbody;
		private SerializedProperty checkController;
		private SerializedProperty checkWireframe;
		private SerializedProperty checkRaycast;

		protected override void FindUniqueDetectorProperties()
		{
			raycastDelay = serializedObject.FindProperty("raycastDelay");
			wireframeDelay = serializedObject.FindProperty("wireframeDelay");
			spawnPosition = serializedObject.FindProperty("spawnPosition");
			maxFalsePositives = serializedObject.FindProperty("maxFalsePositives");

			checkRigidbody = serializedObject.FindProperty("checkRigidbody");
			checkController = serializedObject.FindProperty("checkController");
			checkWireframe = serializedObject.FindProperty("checkWireframe");
			checkRaycast = serializedObject.FindProperty("checkRaycast");
		}

		protected override void DrawUniqueDetectorProperties()
		{
			WallHackDetector detector = self as WallHackDetector;
			if (detector == null) return;

			if (PropertyFieldChanged(checkRigidbody, new GUIContent("Rigidbody")))
			{
				detector.CheckRigidbody = checkRigidbody.boolValue;
			}

			if (PropertyFieldChanged(checkController, new GUIContent("Character Controller")))
			{
				detector.CheckController = checkController.boolValue;
			}

			if (PropertyFieldChanged(checkWireframe, new GUIContent("Wireframe")))
			{
				detector.CheckWireframe = checkWireframe.boolValue;
			}
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(wireframeDelay, new GUIContent("Delay"));
			EditorGUI.indentLevel--;

			if (PropertyFieldChanged(checkRaycast, new GUIContent("Raycast")))
			{
				detector.CheckRaycast = checkRaycast.boolValue;
			}
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(raycastDelay, new GUIContent("Delay"));
			EditorGUI.indentLevel--;

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(spawnPosition);
			EditorGUILayout.PropertyField(maxFalsePositives);

			EditorGUILayout.Separator();

			if (!ActSettings.IsWallhackDetectorShaderIncluded())
			{
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("You need to include specific shader into your build to let WallHackDetector work properly.", EditorStyles.wordWrappedLabel);
				if (GUILayout.Button("Include in Settings..."))
				{
					ActSettings.ShowWindow();
				}
			}

			if (checkRaycast.boolValue || checkController.boolValue || checkRigidbody.boolValue)
			{
				int layerId = LayerMask.NameToLayer("Ignore Raycast");
				if (Physics.GetIgnoreLayerCollision(layerId, layerId))
				{
					EditorGUILayout.LabelField("IgnoreRaycast physics layer should collide with itself to avoid false positives! See readme's troubleshooting section for details.", EditorStyles.wordWrappedLabel);
					if (GUILayout.Button("Edit in Physics settings"))
					{
						EditorApplication.ExecuteMenuItem("Edit/Project Settings/Physics");
                    }
				}
			}
		}

		private static bool PropertyFieldChanged(SerializedProperty property, GUIContent content, params GUILayoutOption[] options)
		{
			bool result = false;

			EditorGUI.BeginChangeCheck();

			if (content == null)
			{
				EditorGUILayout.PropertyField(property, options);
			}
			else
			{
				EditorGUILayout.PropertyField(property, content, options);
			}

			if (EditorGUI.EndChangeCheck())
			{
				result = true;
			}
			return result;
		}
	}
}