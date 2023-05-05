using UnityEditor;

namespace PathfindingCore.Legacy {
	[CustomCoreComponentInspector(typeof(LegacyRVOController))]
	[CanEditMultipleObjects]
	public class LegacyRVOControllerEditor : PathfindingCore.RVO.RVOControllerEditor {
		protected override void Inspector () {
			DrawDefaultInspector();

			//LegacyEditorHelper.UpgradeDialog(targets, typeof(Pathfinding.RVO.RVOController));
			EditorGUILayout.LabelField($"[{this.GetType().Name}] Not Complement yet");
		}
	}
}
