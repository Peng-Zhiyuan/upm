using UnityEditor;

namespace PathfindingCore.Legacy {
	[CustomCoreComponentInspector(typeof(LegacyAIPath))]
	[CanEditMultipleObjects]
	public class LegacyAIPathEditor : BaseAIEditor {
		protected override void Inspector () {
			base.Inspector();

			EditorGUILayout.LabelField($"[{this.GetType().Name}] Not Complement yet");
			//var gravity = FindProperty("gravity");
			//if (!gravity.hasMultipleDifferentValues && !float.IsNaN(gravity.vector3Value.x)) {
			//	gravity.vector3Value = new UnityEngine.Vector3(float.NaN, float.NaN, float.NaN);
			//	serializedObject.ApplyModifiedPropertiesWithoutUndo();
			//}
			//LegacyEditorHelper.UpgradeDialog(targets, typeof(AIPath));
		}
	}
}
