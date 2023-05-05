using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_unit_builder.php")]
	public class RTSUnitBuilder : MonoBehaviour {
		[System.Serializable]
		public class BuildingItem {
			public GameObject prefab;
			public int cost;
			public RTSUI.MenuItem menuItem;
		}

		public BuildingItem[] items;
		RTSUnit unit;
		RTSUI.Menu menu;

		void Awake () {
			unit = GetComponent<RTSUnit>();

			unit.onMakeActiveUnit += (bool active) => {
				if (active) {
					menu = RTSUI.active.ShowMenu();
					for (int i = 0; i < items.Length; i++) {
						var item = items[i];
						menu.AddItem(item.menuItem, () => {
							RTSUI.active.StartBuildingPlacement(item);
						});
					}
				} else if (menu != null) menu.Hide();
			};
		}
	}
}
