using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PathfindingCore {
	[CustomCoreComponentInspector(typeof(AnimationLink))]
	public class AnimationLinkEditor : CoreComponentInspector {
		public override void OnInspectorGUI () {
			DrawDefaultInspector() ;

			var script = target as AnimationLink;

			EditorGUI.BeginDisabledGroup(script.EndTransform == null);
			if (GUILayout.Button("Autoposition Endpoint")) {
				List<Vector3> buffer = PathfindingCore.Util.ListPool<Vector3>.Claim();
				Vector3 endpos;
				script.CalculateOffsets(buffer, out endpos);
				script.EndTransform.position = endpos;
				PathfindingCore.Util.ListPool<Vector3>.Release(buffer);
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}
