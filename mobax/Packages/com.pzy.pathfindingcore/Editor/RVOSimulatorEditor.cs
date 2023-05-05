using UnityEditor;
using UnityEngine;

namespace PathfindingCore {
	[CustomCoreComponentInspector(typeof(PathfindingCore.RVO.RVOSimulator))]
	public class RVOSimulatorEditor : EditorBase {
		static readonly GUIContent[] movementPlaneOptions = new [] { new GUIContent("XZ (for 3D games)"), new GUIContent("XY (for 2D games)"), new GUIContent("Arbitrary (for non-planar worlds)") };

		protected override void Inspector () {
			Section("Movement");
			Popup("movementPlane", movementPlaneOptions);

			PropertyField("symmetryBreakingBias");
			PropertyField("hardCollisions");

			Section("Execution");
			PropertyField("desiredSimulationFPS");
			ClampInt("desiredSimulationFPS", 1);
			PropertyField("doubleBuffering");

			Section("Debugging");
			PropertyField("drawObstacles");

			if (FindProperty("doubleBuffering").boolValue && FindProperty("hardCollisions").boolValue) {
				EditorGUILayout.HelpBox("It is not recommended to use both double buffering and hard collisions due to this easily leading to excessive jitter in the agents' movement.", MessageType.Warning);
			}

			var burstSim = (target as PathfindingCore.RVO.RVOSimulator).GetSimulator() as PathfindingCore.RVO.SimulatorBurst;
			if (burstSim != null && burstSim.AnyAgentHasDebug && burstSim.DoubleBuffering) {
				EditorGUILayout.HelpBox("At least one agent has debug drawing enabled. This prevents double buffering from being used.", MessageType.Warning);
			}
		}
	}
}
