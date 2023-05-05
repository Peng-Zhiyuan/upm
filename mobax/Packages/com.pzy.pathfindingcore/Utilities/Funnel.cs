using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingCore {
	using PathfindingCore.Util;

	/// <summary>
	/// Implements the funnel algorithm as well as various related methods.
	/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
	/// See: FunnelModifier for the component that you can attach to objects to use the funnel algorithm.
	/// </summary>
	public class Funnel {
		/// <summary>Funnel in which the path to the target will be</summary>
		public struct FunnelPortals {
			public List<Vector3> left;
			public List<Vector3> right;
		}

		/// <summary>
		/// Part of a path.
		/// This is either a sequence of adjacent triangles
		/// or a link.
		/// See: NodeLink2
		/// </summary>
		public struct PathPart {
			/// <summary>Index of the first node in this part</summary>
			public int startIndex;
			/// <summary>Index of the last node in this part</summary>
			public int endIndex;
			public Vector3 startPoint, endPoint;
			public bool isLink;
		}

		public static List<PathPart> SplitIntoParts (Path path) {
			var nodes = path.path;

			var result = ListPool<PathPart>.Claim();

			if (nodes == null || nodes.Count == 0) {
				return result;
			}

			// Loop through the path and split it into
			// parts joined by links
			for (int i = 0; i < nodes.Count; i++) {
				if (nodes[i] is TriangleMeshNode || nodes[i] is GridNodeBase) {
					var part = new PathPart();
					part.startIndex = i;
					uint currentGraphIndex = nodes[i].GraphIndex;

                    // Loop up until we find a node in another graph
                    // Ignore NodeLink3 nodes
                    for (; i < nodes.Count; i++)
                    {
						// pzy:
						// 暂时注释掉了这段
						// 不考虑跨图
                        //if (nodes[i].GraphIndex != currentGraphIndex && !(nodes[i] is NodeLink3Node))
                        //{
                        //    break;
                        //}
                    }

                    i--;
					part.endIndex = i;

					// If this is the first part in the path, use the exact start point
					// otherwise use the position of the node right before the start of this
					// part which is likely the end of the link to this part
					if (part.startIndex == 0) {
						part.startPoint = path.vectorPath[0];
					} else {
						part.startPoint = (Vector3)nodes[part.startIndex-1].position;
					}

					if (part.endIndex == nodes.Count-1) {
						part.endPoint = path.vectorPath[path.vectorPath.Count-1];
					} else {
						part.endPoint = (Vector3)nodes[part.endIndex+1].position;
					}

					result.Add(part);
				} else if (NodeLink2.GetNodeLink(nodes[i]) != null) {
					var part = new PathPart();
					part.startIndex = i;
					var currentGraphIndex = nodes[i].GraphIndex;

					for (i++; i < nodes.Count; i++) {
						if (nodes[i].GraphIndex != currentGraphIndex) {
							break;
						}
					}
					i--;

					if (i - part.startIndex == 0) {
						// Just ignore it, it might be the case that a NodeLink was the closest node
						continue;
					} else if (i - part.startIndex != 1) {
						throw new System.Exception("NodeLink2 link length greater than two (2) nodes. " + (i - part.startIndex + 1));
					}

					part.endIndex = i;
					part.isLink = true;
					part.startPoint = (Vector3)nodes[part.startIndex].position;
					part.endPoint = (Vector3)nodes[part.endIndex].position;
					result.Add(part);
				} else {
					throw new System.Exception("Unsupported node type or null node");
				}
			}

			return result;
		}


		public static void Simplify (List<PathPart> parts, ref List<GraphNode> nodes) {
			List<GraphNode> resultNodes = ListPool<GraphNode>.Claim();

			for (int i = 0; i < parts.Count; i++) {
				var part = parts[i];

				// We are changing the nodes list, so indices may change
				var newPart = part;
				newPart.startIndex = resultNodes.Count;

				if (!part.isLink) {
					var graph = nodes[part.startIndex].Graph as IRaycastableGraph;
					if (graph != null) {
						Simplify(part, graph, nodes, resultNodes, Path.ZeroTagPenalties, -1);
						newPart.endIndex = resultNodes.Count - 1;
						parts[i] = newPart;
						continue;
					}
				}

				for (int j = part.startIndex; j <= part.endIndex; j++) {
					resultNodes.Add(nodes[j]);
				}
				newPart.endIndex = resultNodes.Count - 1;
				parts[i] = newPart;
			}

			ListPool<GraphNode>.Release(ref nodes);
			nodes = resultNodes;
		}

		/// <summary>
		/// Simplifies a funnel path using linecasting.
		/// Running time is roughly O(n^2 log n) in the worst case (where n = end-start)
		/// Actually it depends on how the graph looks, so in theory the actual upper limit on the worst case running time is O(n*m log n) (where n = end-start and m = nodes in the graph)
		/// but O(n^2 log n) is a much more realistic worst case limit.
		///
		/// Requires <see cref="graph"/> to implement IRaycastableGraph
		/// </summary>
		public static void Simplify (PathPart part, IRaycastableGraph graph, List<GraphNode> nodes, List<GraphNode> result, int[] tagPenalties, int traversableTags) {
			var start = part.startIndex;
			var end = part.endIndex;
			var startPoint = part.startPoint;
			var endPoint = part.endPoint;

			if (graph == null) throw new System.ArgumentNullException(nameof(graph));

			if (start > end) {
				throw new System.ArgumentException("start > end");
			}

			// Do a straight line of sight check to see if the path can be simplified to a single line
			{
				GraphHitInfo hit;
				if (!graph.Linecast(startPoint, endPoint, nodes[start], out hit) && hit.node == nodes[end]) {
					graph.Linecast(startPoint, endPoint, nodes[start], out hit, result);

					long penaltySum = 0;
					long penaltySum2 = 0;
					for (int i = start; i <= end; i++) {
						penaltySum += nodes[i].Penalty + tagPenalties[nodes[i].Tag];
					}

					bool walkable = true;
					for (int i = 0; i < result.Count; i++) {
						penaltySum2 += result[i].Penalty + tagPenalties[result[i].Tag];
						walkable &= ((traversableTags >> (int)result[i].Tag) & 1) == 1;
					}

					// Allow 40% more penalty on average per node
					if (!walkable || (penaltySum*1.4*result.Count) < (penaltySum2*(end-start+1))) {
						// The straight line penalties are much higher than the original path.
						// Revert the simplification
						result.Clear();
					} else {
						// The straight line simplification looks good.
						// We are done here.
						return;
					}
				}
			}

			int ostart = start;

			int count = 0;
			while (true) {
				if (count++ > 1000) {
					Debug.LogError("Was the path really long or have we got cought in an infinite loop?");
					break;
				}

				if (start == end) {
					result.Add(nodes[end]);
					return;
				}

				int resCount = result.Count;

				// Run a binary search to find the furthest node that we have a clear line of sight to
				int mx = end+1;
				int mn = start+1;
				bool anySucceded = false;
				while (mx > mn+1) {
					int mid = (mx+mn)/2;

					GraphHitInfo hit;
					Vector3 sp = start == ostart ? startPoint : (Vector3)nodes[start].position;
					Vector3 ep = mid == end ? endPoint : (Vector3)nodes[mid].position;

					// Check if there is an obstacle between these points, or if there is no obstacle, but we didn't end up at the right node.
					// The second case can happen for example in buildings with multiple floors.
					if (graph.Linecast(sp, ep, nodes[start], out hit) || hit.node != nodes[mid]) {
						mx = mid;
					} else {
						anySucceded = true;
						mn = mid;
					}
				}

				if (!anySucceded) {
					result.Add(nodes[start]);

					// It is guaranteed that mn = start+1
					start = mn;
				} else {
					// Replace a part of the path with the straight path to the furthest node we had line of sight to.
					// Need to redo the linecast to get the trace (i.e. list of nodes along the line of sight).
					GraphHitInfo hit;
					Vector3 sp = start == ostart ? startPoint : (Vector3)nodes[start].position;
					Vector3 ep = mn == end ? endPoint : (Vector3)nodes[mn].position;
					graph.Linecast(sp, ep, nodes[start], out hit, result);

					long penaltySum = 0;
					long penaltySum2 = 0;
					for (int i = start; i <= mn; i++) {
						penaltySum += nodes[i].Penalty + tagPenalties[nodes[i].Tag];
					}

					bool walkable = true;
					for (int i = resCount; i < result.Count; i++) {
						penaltySum2 += result[i].Penalty + tagPenalties[result[i].Tag];
						walkable &= ((traversableTags >> (int)result[i].Tag) & 1) == 1;
					}

					// Allow 40% more penalty on average per node
					if (!walkable || (penaltySum*1.4*(result.Count-resCount)) < (penaltySum2*(mn-start+1)) || result[result.Count-1] != nodes[mn]) {
						//Debug.DrawLine ((Vector3)nodes[start].Position, (Vector3)nodes[mn].Position, Color.red);
						// Linecast hit the wrong node or it is a lot more expensive than the original path
						result.RemoveRange(resCount, result.Count-resCount);

						result.Add(nodes[start]);
						//Debug.Break();
						start = start+1;
					} else {
						//Debug.DrawLine ((Vector3)nodes[start].Position, (Vector3)nodes[mn].Position, Color.green);
						//Remove nodes[end]
						result.RemoveAt(result.Count-1);
						start = mn;
					}
				}
			}
		}

		public static FunnelPortals ConstructFunnelPortals (List<GraphNode> nodes, PathPart part) {
			if (nodes == null || nodes.Count == 0) {
				return new FunnelPortals { left = ListPool<Vector3>.Claim(0), right = ListPool<Vector3>.Claim(0) };
			}

			if (part.endIndex < part.startIndex || part.startIndex < 0 || part.endIndex > nodes.Count) throw new System.ArgumentOutOfRangeException();

			// Claim temporary lists and try to find lists with a high capacity
			var left = ListPool<Vector3>.Claim(nodes.Count+1);
			var right = ListPool<Vector3>.Claim(nodes.Count+1);

			// Add start point
			left.Add(part.startPoint);
			right.Add(part.startPoint);

			// Loop through all nodes in the path (except the last one)
			for (int i = part.startIndex; i < part.endIndex; i++) {
				// Get the portal between path[i] and path[i+1] and add it to the left and right lists
				bool portalWasAdded = nodes[i].GetPortal(nodes[i+1], left, right, false);

				if (!portalWasAdded) {
					// Fallback, just use the positions of the nodes
					left.Add((Vector3)nodes[i].position);
					right.Add((Vector3)nodes[i].position);

					left.Add((Vector3)nodes[i+1].position);
					right.Add((Vector3)nodes[i+1].position);
				}
			}

			// Add end point
			left.Add(part.endPoint);
			right.Add(part.endPoint);

			return new FunnelPortals { left = left, right = right };
		}

		public static void ShrinkPortals (FunnelPortals portals, float shrink) {
			if (shrink <= 0.00001f) return;

			for (int i = 0; i < portals.left.Count; i++) {
				var left = portals.left[i];
				var right = portals.right[i];

				var length = (left - right).magnitude;
				if (length > 0) {
					float s = Mathf.Min(shrink / length, 0.4f);
					portals.left[i] = Vector3.Lerp(left, right, s);
					portals.right[i] = Vector3.Lerp(left, right, 1 - s);
				}
			}
		}

		static bool UnwrapHelper (Vector3 portalStart, Vector3 portalEnd, Vector3 prevPoint, Vector3 nextPoint, ref Quaternion mRot, ref Vector3 mOffset) {
			// Skip the point if it was on the rotation axis
			if (VectorMath.IsColinear(portalStart, portalEnd, nextPoint)) {
				return false;
			}

			var axis = portalEnd - portalStart;
			var sqrMagn = axis.sqrMagnitude;
			prevPoint -= Vector3.Dot(prevPoint - portalStart, axis)/sqrMagn * axis;
			nextPoint -= Vector3.Dot(nextPoint - portalStart, axis)/sqrMagn * axis;
			var rot = Quaternion.FromToRotation(nextPoint - portalStart, portalStart - prevPoint);

			// The code below is equivalent to these matrix operations (but a lot faster)
			// This represents a rotation around a line in 3D space
			//mat = mat * Matrix4x4.TRS(portalStart, rot, Vector3.one) * Matrix4x4.TRS(-portalStart, Quaternion.identity, Vector3.one);
			mOffset += mRot * (portalStart - rot * portalStart);
			mRot *= rot;

			return true;
		}

		/// <summary>
		/// Unwraps the funnel portals from 3D space to 2D space.
		/// The result is stored in the left and right arrays which must be at least as large as the funnel.left and funnel.right lists.
		///
		/// The input is a funnel like in the image below. It may be rotated and twisted.
		/// [Open online documentation to see images]
		/// The output will be a funnel in 2D space like in the image below. All twists and bends will have been straightened out.
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Calculate(FunnelPortals,bool,bool)"/>
		/// </summary>
		public static void Unwrap (FunnelPortals funnel, Vector2[] left, Vector2[] right) {
			int startingIndex = 1;
			var normal = Vector3.Cross(funnel.right[1] - funnel.left[0], funnel.left[1] - funnel.left[0]);

			// This handles the case when the starting point is colinear with the first portal.
			// Note that left.Length is only guaranteed to be at least as large as funnel.left.Count, it may be larger.
			while (normal.sqrMagnitude <= 0.00000001f && startingIndex + 1 < funnel.left.Count) {
				startingIndex++;
				normal = Vector3.Cross(funnel.right[startingIndex] - funnel.left[0], funnel.left[startingIndex] - funnel.left[0]);
			}

			left[0] = right[0] = Vector2.zero;

			var portalLeft = funnel.left[1];
			var portalRight = funnel.right[1];
			var prevPoint = funnel.left[0];

			// The code below is equivalent to this matrix (but a lot faster)
			// This represents a rotation around a line in 3D space
			// Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(normal, Vector3.forward), Vector3.one) * Matrix4x4.TRS(-funnel.right[0], Quaternion.identity, Vector3.one);
			Quaternion mRot = Quaternion.FromToRotation(normal, Vector3.forward);
			Vector3 mOffset = mRot * (-funnel.right[0]);

			for (int i = 1; i < funnel.left.Count; i++) {
				if (UnwrapHelper(portalLeft, portalRight, prevPoint, funnel.left[i], ref mRot, ref mOffset)) {
					prevPoint = portalLeft;
					portalLeft = funnel.left[i];
				}

				left[i] = mRot * funnel.left[i] + mOffset;

				if (UnwrapHelper(portalLeft, portalRight, prevPoint, funnel.right[i], ref mRot, ref mOffset)) {
					prevPoint = portalRight;
					portalRight = funnel.right[i];
				}

				right[i] = mRot * funnel.right[i] + mOffset;
			}
		}

		/// <summary>
		/// Try to fix degenerate or invalid funnels.
		/// Returns: The number of vertices at the start of both arrays that should be ignored or -1 if the algorithm failed.
		/// </summary>
		static int FixFunnel (Vector2[] left, Vector2[] right, int numPortals) {
			if (numPortals > left.Length || numPortals > right.Length) throw new System.ArgumentException("Arrays do not have as many elements as specified");

			if (numPortals < 3) {
				return -1;
			}

			// Remove duplicate vertices
			int startIndex = 0;
			while (left[startIndex + 1] == left[startIndex + 2] && right[startIndex + 1] == right[startIndex + 2]) {
				// Equivalent to RemoveAt(1) if they would have been lists
				left[startIndex + 1] = left[startIndex + 0];
				right[startIndex + 1] = right[startIndex + 0];
				startIndex++;

				if (numPortals - startIndex < 3) {
					return -1;
				}
			}

			return startIndex;
		}

		protected static Vector2 ToXZ (Vector3 p) {
			return new Vector2(p.x, p.z);
		}

		protected static Vector3 FromXZ (Vector2 p) {
			return new Vector3(p.x, 0, p.y);
		}

		/// <summary>True if b is to the right of or on the line from (0,0) to a</summary>
		protected static bool RightOrColinear (Vector2 a, Vector2 b) {
			return (a.x*b.y - b.x*a.y) <= 0;
		}

		/// <summary>True if b is to the left of or on the line from (0,0) to a</summary>
		protected static bool LeftOrColinear (Vector2 a, Vector2 b) {
			return (a.x*b.y - b.x*a.y) >= 0;
		}

		/// <summary>
		/// Calculate the shortest path through the funnel.
		///
		/// If the unwrap option is disabled the funnel will simply be projected onto the XZ plane.
		/// If the unwrap option is enabled then the funnel may be oriented arbitrarily and may have twists and bends.
		/// This makes it possible to support the funnel algorithm in XY space as well as in more complicated cases, such
		/// as on curved worlds.
		/// [Open online documentation to see images]
		///
		/// [Open online documentation to see images]
		///
		/// See: Unwrap
		/// </summary>
		/// <param name="funnel">The portals of the funnel. The first and last vertices portals must be single points (so for example left[0] == right[0]).</param>
		/// <param name="unwrap">Determines if twists and bends should be straightened out before running the funnel algorithm.</param>
		/// <param name="splitAtEveryPortal">If true, then a vertex will be inserted every time the path crosses a portal
		///  instead of only at the corners of the path. The result will have exactly one vertex per portal if this is enabled.
		///  This may introduce vertices with the same position in the output (esp. in corners where many portals meet).</param>
		public static List<Vector3> Calculate (FunnelPortals funnel, bool unwrap, bool splitAtEveryPortal) {
			if (funnel.left.Count != funnel.right.Count) throw new System.ArgumentException("funnel.left.Count != funnel.right.Count");

			// Get arrays at least as large as the number of portals
			var leftArr = ArrayPool<Vector2>.Claim(funnel.left.Count);
			var rightArr = ArrayPool<Vector2>.Claim(funnel.left.Count);

			if (unwrap) {
				Unwrap(funnel, leftArr, rightArr);
			} else {
				// Copy to arrays
				for (int i = 0; i < funnel.left.Count; i++) {
					leftArr[i] = ToXZ(funnel.left[i]);
					rightArr[i] = ToXZ(funnel.right[i]);
				}
			}

			int startIndex = FixFunnel(leftArr, rightArr, funnel.left.Count);
			var intermediateResult = ListPool<int>.Claim();
			if (startIndex == -1) {
				// If funnel algorithm failed, fall back to a simple line
				intermediateResult.Add(0);
				intermediateResult.Add(funnel.left.Count - 1);
			} else {
				bool lastCorner;
				Calculate(leftArr, rightArr, funnel.left.Count, startIndex, intermediateResult, int.MaxValue, out lastCorner);
			}

			// Get list for the final result
			var result = ListPool<Vector3>.Claim(intermediateResult.Count);

			Vector2 prev2D = leftArr[0];
			var prevIdx = 0;
			for (int i = 0; i < intermediateResult.Count; i++) {
				var idx = intermediateResult[i];

				if (splitAtEveryPortal) {
					// Check intersections with every portal segment
					var next2D = idx >= 0 ? leftArr[idx] : rightArr[-idx];
					for (int j = prevIdx + 1; j < System.Math.Abs(idx); j++) {
						var factor = VectorMath.LineIntersectionFactorXZ(FromXZ(leftArr[j]), FromXZ(rightArr[j]), FromXZ(prev2D), FromXZ(next2D));
						result.Add(Vector3.Lerp(funnel.left[j], funnel.right[j], factor));
					}

					prevIdx = Mathf.Abs(idx);
					prev2D = next2D;
				}

				if (idx >= 0) {
					result.Add(funnel.left[idx]);
				} else {
					result.Add(funnel.right[-idx]);
				}
			}

			// Release lists back to the pool
			ListPool<int>.Release(ref intermediateResult);
			ArrayPool<Vector2>.Release(ref leftArr);
			ArrayPool<Vector2>.Release(ref rightArr);
			return result;
		}

		/// <summary>
		/// Funnel algorithm.
		/// funnelPath will be filled with the result.
		/// The result is the indices of the vertices that were picked, a non-negative value refers to the corresponding index in the
		/// left array, a negative value refers to the corresponding index in the right array.
		/// So e.g 5 corresponds to left[5] and -2 corresponds to right[2]
		///
		/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
		/// </summary>
		static void Calculate (Vector2[] left, Vector2[] right, int numPortals, int startIndex, List<int> funnelPath, int maxCorners, out bool lastCorner) {
			if (left.Length != right.Length) throw new System.ArgumentException();

			lastCorner = false;

			int apexIndex = startIndex + 0;
			int rightIndex = startIndex + 1;
			int leftIndex = startIndex + 1;

			Vector2 portalApex = left[apexIndex];
			Vector2 portalLeft = left[leftIndex];
			Vector2 portalRight = right[rightIndex];

			funnelPath.Add(apexIndex);

			for (int i = startIndex + 2; i < numPortals; i++) {
				if (funnelPath.Count >= maxCorners) {
					return;
				}

				if (funnelPath.Count > 2000) {
					Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
					break;
				}

				Vector2 pLeft = left[i];
				Vector2 pRight = right[i];

				if (LeftOrColinear(portalRight - portalApex, pRight - portalApex)) {
					if (portalApex == portalRight || RightOrColinear(portalLeft - portalApex, pRight - portalApex)) {
						portalRight = pRight;
						rightIndex = i;
					} else {
						portalApex = portalRight = portalLeft;
						i = apexIndex = rightIndex = leftIndex;

						funnelPath.Add(apexIndex);
						continue;
					}
				}

				if (RightOrColinear(portalLeft - portalApex, pLeft - portalApex)) {
					if (portalApex == portalLeft || LeftOrColinear(portalRight - portalApex, pLeft - portalApex)) {
						portalLeft = pLeft;
						leftIndex = i;
					} else {
						portalApex = portalLeft = portalRight;
						i = apexIndex = leftIndex = rightIndex;

						// Negative value because we are referring
						// to the right side
						funnelPath.Add(-apexIndex);

						continue;
					}
				}
			}

			lastCorner = true;
			funnelPath.Add(numPortals-1);
		}
	}
}
