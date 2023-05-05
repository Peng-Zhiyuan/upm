using UnityEngine;
using System.Collections.Generic;

namespace PathfindingCore {
	/// <summary>
	/// Movement script for curved worlds.
	/// This script inherits from AIPath, but adjusts its movement plane every frame using the ground normal.
	/// </summary>
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_a_i_path_aligned_to_surface.php")]
	public class AIPathAlignedToSurface : AIPath {
		public override void OnCoreStart () {
			base.OnCoreStart();
			movementPlane = new Util.SimpleMovementPlane(rotation);
		}

		protected override void OnUpdate (float dt) {
			base.OnUpdate(dt);
			UpdateMovementPlane();
		}

		protected override void ApplyGravity (float deltaTime) {
			// Apply gravity
			if (usingGravity) {
				// Gravity is relative to the current surface.
				// Only the normal direction is well defined however so x and z are ignored.
				verticalVelocity += float.IsNaN(gravity.x) ? Physics.gravity.y : gravity.y;
			} else {
				verticalVelocity = 0;
			}
		}

		Mesh cachedMesh;
		List<Vector3> cachedNormals = new List<Vector3>();
		List<int> cachedTriangles = new List<int>();
		Vector3 InterpolateNormal (RaycastHit hit) {
			MeshCollider meshCollider = hit.collider as MeshCollider;

			if (meshCollider == null || meshCollider.sharedMesh == null)
				return hit.normal;

			Mesh mesh = meshCollider.sharedMesh;

			// For performance, cache the triangles and normals from the last frame
			if (mesh != cachedMesh) {
				if (!mesh.isReadable) return hit.normal;
				cachedMesh = mesh;
				mesh.GetNormals(cachedNormals);
				if (mesh.subMeshCount == 1) {
					mesh.GetTriangles(cachedTriangles, 0);
				} else {
					List<int> buffer = PathfindingCore.Util.ListPool<int>.Claim();
					// Absolutely horrible, but there doesn't seem to be another way to do this without allocating a ton of memory each time
					for (int i = 0; i < mesh.subMeshCount; i++) {
						mesh.GetTriangles(buffer, i);
						cachedTriangles.AddRange(buffer);
					}
					PathfindingCore.Util.ListPool<int>.Release(ref buffer);
				}
			}

			var normals = cachedNormals;
			var triangles = cachedTriangles;
			Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
			Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
			Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
			Vector3 baryCenter = hit.barycentricCoordinate;
			Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
			interpolatedNormal = interpolatedNormal.normalized;
			Transform hitTransform = hit.collider.transform;
			interpolatedNormal = hitTransform.TransformDirection(interpolatedNormal);
			return interpolatedNormal;
		}


		/// <summary>Find the world position of the ground below the character</summary>
		protected override void UpdateMovementPlane () {
			// Construct a new movement plane which has new normal
			// but is otherwise as similar to the previous plane as possible
			var normal = InterpolateNormal(lastRaycastHit);

			if (normal != Vector3.zero) {
				var fwd = Vector3.Cross(movementPlane.rotation * Vector3.right, normal);
				movementPlane = new Util.SimpleMovementPlane(Quaternion.LookRotation(fwd, normal));
			}
			rvoController.movementPlane = movementPlane;
		}
	}
}
