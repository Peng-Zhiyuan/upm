using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace PathfindingCore.Voxels.Burst {
	/// <summary>Stores a compact voxel field. </summary>
	public struct CompactVoxelField {
		public const int UnwalkableArea = 0;
		public const uint NotConnected = 0x3f;
		public int voxelWalkableHeight;
		public int width, depth;
		public NativeList<CompactVoxelSpan> spans;
		public NativeList<CompactVoxelCell> cells;
		public NativeList<int> areaTypes;

		/// <summary>Unmotivated variable, but let's clamp the layers at 65535</summary>
		public const int MaxLayers = 65535;

		public CompactVoxelField (int width, int depth, int voxelWalkableHeight, Allocator allocator) {
			spans = new NativeList<CompactVoxelSpan>(0, allocator);
			cells = new NativeList<CompactVoxelCell>(0, allocator);
			areaTypes = new NativeList<int>(0, allocator);
			this.width = width;
			this.depth = depth;
			this.voxelWalkableHeight = voxelWalkableHeight;
		}

		public void Dispose () {
			spans.Dispose();
			cells.Dispose();
			areaTypes.Dispose();
		}

		public int GetNeighbourIndex (int index, int direction) {
			return index + VoxelUtilityBurst.DX[direction] + VoxelUtilityBurst.DZ[direction] * width;
		}

		public void BuildFromLinkedField (LinkedVoxelField field) {
			int idx = 0;

			Assert.AreEqual(this.width, field.width);
			Assert.AreEqual(this.depth, field.depth);

			this.depth = field.depth;

			int w = field.width;
			int d = field.depth;
			int wd = w*d;

			int spanCount = field.GetSpanCount();
			spans.Resize(spanCount, NativeArrayOptions.UninitializedMemory);
			areaTypes.Resize(spanCount, NativeArrayOptions.UninitializedMemory);
			cells.Resize(wd, NativeArrayOptions.UninitializedMemory);

			if (this.voxelWalkableHeight >= ushort.MaxValue) {
				throw new System.Exception("Too high walkable height to guarantee correctness. Increase voxel height or lower walkable height.");
			}

			var linkedSpans = field.linkedSpans;
			for (int z = 0, pz = 0; z < wd; z += w, pz++) {
				for (int x = 0; x < w; x++) {
					int spanIndex = x+z;
					if (linkedSpans[spanIndex].bottom == LinkedVoxelField.InvalidSpanValue) {
						cells[x+z] = new CompactVoxelCell(0, 0);
						continue;
					}

					int index = idx;
					int count = 0;

					while (spanIndex != -1) {
						if (linkedSpans[spanIndex].area != UnwalkableArea) {
							int bottom = (int)linkedSpans[spanIndex].top;
							int next = linkedSpans[spanIndex].next;
							int top = next != -1 ? (int)linkedSpans[next].bottom : VoxelArea.MaxHeightInt;

							// TODO: Why is top-bottom clamped to a ushort range?
							spans[idx] = new CompactVoxelSpan((ushort)math.min(bottom, ushort.MaxValue), (uint)math.min(top-bottom, ushort.MaxValue));
							areaTypes[idx] = linkedSpans[spanIndex].area;
							idx++;
							count++;
						}
						spanIndex = linkedSpans[spanIndex].next;
					}

					cells[x+z] = new CompactVoxelCell(index, count);
				}
			}

			if (idx != spanCount) throw new System.Exception("Found span count does not match expected value");
		}
	}

	/// <summary>CompactVoxelCell used for recast graphs.</summary>
	public struct CompactVoxelCell {
		public int index;
		public int count;

		public CompactVoxelCell (int i, int c) {
			index = i;
			count = c;
		}
	}

	/// <summary>CompactVoxelSpan used for recast graphs.</summary>
	public struct CompactVoxelSpan {
		public ushort y;
		public uint con;
		public uint h;
		public int reg;

		public CompactVoxelSpan (ushort bottom, uint height) {
			con = 24;
			y = bottom;
			h = height;
			reg = 0;
		}

		public void SetConnection (int dir, uint value) {
			int shift = dir*6;

			con  = (uint)((con & ~(0x3f << shift)) | ((value & 0x3f) << shift));
		}

		public int GetConnection (int dir) {
			return ((int)con >> dir*6) & 0x3f;
		}
	}
}
