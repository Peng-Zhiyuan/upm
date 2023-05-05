using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_resource_deterioration.php")]
	public class RTSResourceDeterioration : MonoBehaviour {
		RTSHarvestableResource resource;
		public Transform offsetRoot;
		public float maxOffset;
		float initialResources;

		void Start () {
			resource = GetComponent<RTSHarvestableResource>();
			initialResources = resource.value;
		}

		// Update is called once per frame
		void Update () {
			var offset = Mathf.Clamp((initialResources - resource.value) / Mathf.Max(1f, initialResources), 0.0f, 1.0f);

			if (resource.value > 0) {
				offset *= 0.8f;
			}

			offsetRoot.localPosition = Vector3.Lerp(offsetRoot.localPosition, Vector3.down * offset * maxOffset, 2 * Time.deltaTime);
		}
	}
}
