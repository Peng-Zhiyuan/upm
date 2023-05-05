using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_harvestable_resource.php")]
	public class RTSHarvestableResource : MonoBehaviour {
		public float value;
		public ResourceType resourceType;

		public bool harvestable {
			get {
				return value > 0;
			}
		}
	}

	public enum ResourceType {
		Crystal
	}
}
