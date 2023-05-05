using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PathfindingCore {
	[CustomEditor(typeof(ProceduralGridMover))]
	[CanEditMultipleObjects]
	public class ProceduralGridMoverEditor : EditorBase {
		GUIContent[] graphLabels = new GUIContent[32];

		protected override void Inspector () {
			// Make sure the AstarPath object is initialized and the graphs are loaded, this is required to be able to show graph names in the mask popup
			AstarPathCore.FindAstarPath();

			for (int i = 0; i < graphLabels.Length; i++) {
				if (AstarPathCore.lastestInstance == null || AstarPathCore.lastestInstance.data.graphs == null || i >= AstarPathCore.lastestInstance.data.graphs.Length || AstarPathCore.lastestInstance.data.graphs[i] == null) {
					graphLabels[i] = new GUIContent("Graph " + i + (i == 31 ? "+" : ""));
				} else {
					graphLabels[i] = new GUIContent(AstarPathCore.lastestInstance.data.graphs[i].name + " (graph " + i + ")");
				}
			}

			Popup("graphIndex", graphLabels, "Graph");
			PropertyField("target");
			PropertyField("updateDistance");
		}
	}
}
