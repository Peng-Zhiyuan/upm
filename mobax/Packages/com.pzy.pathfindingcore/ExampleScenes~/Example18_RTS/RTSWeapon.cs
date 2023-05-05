using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_weapon.php")]
	public class RTSWeapon : MonoBehaviour {
		public bool ranged;
		public float range;
		public float cooldown;
		public float attackDuration;
		public bool canMoveWhileAttacking = false;

		float lastAttackTime = float.NegativeInfinity;

		public virtual bool Aim (RTSUnit target) {
			return Time.time - lastAttackTime >= cooldown;
		}

		public bool isAttacking {
			get {
				return Time.time - lastAttackTime < attackDuration;
			}
		}

		public bool InRangeOf (Vector3 point) {
			return (transform.position - point).sqrMagnitude < range*range;
		}

		public virtual void Attack (RTSUnit target) {
			lastAttackTime = Time.time;
		}
	}
}
