using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_timed_destruction.php")]
	public class RTSTimedDestruction : MonoBehaviour {
		public float time = 1f;

		// Use this for initialization
		IEnumerator Start () {
			yield return new WaitForSeconds(time);
			GameObject.Destroy(gameObject);
		}
	}
}
