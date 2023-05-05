using UnityEngine;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_manager.php")]
	public class RTSManager : VersionedMonoBehaviour {
		public static RTSManager instance;

		public RTSUnitManager units;
		public new RTSAudio audio;

		RTSPlayer[] players;

		protected override void Awake () {
			if (instance != null) throw new System.Exception("Multiple RTSManager instances in the scene. You should only have one.");
			instance = this;

			units = new RTSUnitManager();
			units.Awake();

			players = new RTSPlayer[3];
			for (int i = 0; i < players.Length; i++) {
				players[i] = new RTSPlayer();
				players[i].index = i;
			}
		}

		void OnDestroy () {
			units.OnDestroy();
			instance = null;
		}

		public int PlayerCount => players.Length;

		public RTSPlayer GetPlayer (int team) {
			return players[team];
		}
	}
}
