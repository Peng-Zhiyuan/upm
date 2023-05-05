using UnityEngine;
using System.Collections.Generic;
using PathfindingCore.Util;

namespace PathfindingCore {
	[AddComponentMenu("Pathfinding/Modifiers/Custom")]
	[System.Serializable]
	/// <summary>
	/// Simplifies paths on navmesh graphs using the funnel algorithm.
	/// The funnel algorithm is an algorithm which can, given a path corridor with nodes in the path where the nodes have an area, like triangles, it can find the shortest path inside it.
	/// This makes paths on navmeshes look much cleaner and smoother.
	/// [Open online documentation to see images]
	///
	/// The funnel modifier also works on grid graphs however since it only simplifies the paths within the nodes which the original path visited it may not always
	/// simplify the path as much as you would like it to. The <see cref="PathfindingCore.RaycastModifier"/> can be a better fit for grid graphs.
	/// [Open online documentation to see images]
	///
	/// \ingroup modifiers
	/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
	/// </summary>
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_funnel_modifier.php")]
	public class CustomModifier : MonoModifier {
		/// <summary>
		/// Determines if twists and bends should be straightened out before running the funnel algorithm.
		/// If the unwrap option is disabled the funnel will simply be projected onto the XZ plane.
		/// If the unwrap option is enabled then the funnel may be oriented arbitrarily and may have twists and bends.
		/// This makes it possible to support the funnel algorithm in XY space as well as in more complicated cases, such
		/// as on curved worlds.
		///
		/// Note: This has a performance overhead, so if you do not need it you can disable it to improve
		/// performance.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="PathfindingCore.Funnel.Unwrap"/> for more example images.
		///
		/// Note: This is required if you want to use the funnel modifier for 2D games (i.e in the XY plane).
		/// </summary>
		public bool unwrap = true;

		/// <summary>
		/// Insert a vertex every time the path crosses a portal instead of only at the corners of the path.
		/// The resulting path will have exactly one vertex per portal if this is enabled.
		/// This may introduce vertices with the same position in the output (esp. in corners where many portals meet).
		/// [Open online documentation to see images]
		/// </summary>
		public bool splitAtEveryPortal;

#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Custom Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(CustomModifier));
		}
#endif

		public override int Order { get { return 10; } }
		public bool HasGround(Vector3 pos)
		{
	
			RaycastHit hit;
			int layer = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Road");
			if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, layer, QueryTriggerInteraction.Ignore))
			{
				return hit.point.y >= -0.5f;
			}
			return false;
		}
		public override void Apply (Path p) {

			if (p.path == null || p.path.Count == 0 || p.vectorPath == null || p.vectorPath.Count == 0 ) {
				return;
			}
			//Debug.LogError("customPath");
			List<Vector3> customPath = ListPool<Vector3>.Claim ();

			for (int i = 0; i < p.vectorPath.Count; i++)
			{
				if (this.HasGround(p.vectorPath[i]))
				{
					customPath.Add(p.vectorPath[i]);
				}
				else
				{
					break;
				}
			}
			//if (p.vectorPath.Count > 0)
			//{
				if (customPath.Count == 1)
				{
					customPath.Add(customPath[0]);
				}
			//this.destination = p.vectorPath[p.vectorPath.Count - 1];
			//	this.interpolator.SetPath(p.vectorPath);
			//}

		


			// Split the path into different parts (separated by custom links)
			// and run the funnel algorithm on each of them in turn
			//var parts = Funnel.SplitIntoParts(p);

			//	if (parts.Count == 0) {
			//		// As a really special case, it might happen that the path contained only a single node
			//		// and that node was part of a custom link (e.g added by the NodeLink2 component).
			//		// In that case the SplitIntoParts method will not know what to do with it because it is
			//		// neither a link (as only 1 of the 2 nodes of the link was part of the path) nor a normal
			//		// path part. So it will skip it. This will cause it to return an empty list.
			//		// In that case we want to simply keep the original path, which is just a single point.
			//		return;
			//	}

			//	for (int i = 0; i < parts.Count; i++) {
			//		var part = parts[i];
			//		if (!part.isLink) {
			//			var portals = Funnel.ConstructFunnelPortals(p.path, part);
			//			var result = Funnel.Calculate(portals, unwrap, splitAtEveryPortal);
			//			customPath.AddRange(result);
			//			ListPool<Vector3>.Release (ref portals.left);
			//			ListPool<Vector3>.Release (ref portals.right);
			//			ListPool<Vector3>.Release (ref result);
			//		} else {
			//			// non-link parts will add the start/end points for the adjacent parts.
			//			// So if there is no non-link part before this one, then we need to add the start point of the link
			//			// and if there is no non-link part after this one, then we need to add the end point.
			//			if (i == 0 || parts[i-1].isLink) {
			//				customPath.Add(part.startPoint);
			//			}
			//			if (i == parts.Count - 1 || parts[i+1].isLink) {
			//				customPath.Add(part.endPoint);
			//			}
			//		}
			//	}
			if (customPath.Count == 0) return;
			//UnityEngine.Assertions.Assert.IsTrue(customPath.Count >= 1);
			//ListPool<Funnel.PathPart>.Release (ref parts);
			// Pool the previous vectorPath
			ListPool<Vector3>.Release (ref p.vectorPath);
			p.vectorPath = customPath;
		}
	}
}
