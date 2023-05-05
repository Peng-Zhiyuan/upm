using CodeStage.AntiCheat.Detectors;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	internal class ActDetectorEditor : Editor
	{
		protected SerializedProperty autoStart;
		protected SerializedProperty autoDispose;
		protected SerializedProperty keepAlive;
		protected SerializedProperty detectionEvent;
		protected SerializedProperty detectionEventHasListener;

		protected ActDetectorBase self;

		public virtual void OnEnable()
		{
			autoStart = serializedObject.FindProperty("autoStart");
			autoDispose = serializedObject.FindProperty("autoDispose");
			keepAlive = serializedObject.FindProperty("keepAlive");
			detectionEvent = serializedObject.FindProperty("detectionEvent");
			detectionEventHasListener = serializedObject.FindProperty("detectionEventHasListener");

			self = target as ActDetectorBase;

			FindUniqueDetectorProperties();
		}

		public override void OnInspectorGUI()
		{
			if (self == null) return;

			serializedObject.Update();

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(autoStart);
			detectionEventHasListener.boolValue = ActEditorGlobalStuff.CheckUnityEventHasActivePersistentListener(detectionEvent);
			if (autoStart.boolValue && !detectionEventHasListener.boolValue)
			{
				EditorGUILayout.LabelField(new GUIContent("You need to add at least one active item to the Detection Event in order to use Auto Start feature!"), ActEditorGUI.BoldLabel);
			}
			else if (!autoStart.boolValue)
			{
				EditorGUILayout.LabelField(new GUIContent("Don't forget to start detection!", "You should start detector from code using ObscuredCheatingDetector.StartDetection() method. See readme for details."), ActEditorGUI.BoldLabel);
				EditorGUILayout.Separator();
			}
			EditorGUILayout.PropertyField(autoDispose);
			EditorGUILayout.PropertyField(keepAlive);

			EditorGUILayout.Separator();

			DrawUniqueDetectorProperties();

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(detectionEvent);
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void FindUniqueDetectorProperties() {}
		protected virtual void DrawUniqueDetectorProperties() {}
	}
}