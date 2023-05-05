using UnityEngine;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace PathfindingCore.RVO.Sampled {
	using PathfindingCore;
	using PathfindingCore.RVO;
	using PathfindingCore.Util;
	using Unity.Burst;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Collections;
	using VelocityObstacle = VO;
	using Unity.IL2CPP.CompilerServices;
	using PathfindingCore.Drawing;

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	public struct JobRVOCopy : IJob {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[WriteOnly]
		public SimulatorBurst.AgentData agentDataCopy;

		[WriteOnly]
		public NativeArray<bool> anyDebugEnabled;

		public int startIndex;
		public int endIndex;

		public void Execute () {
			agentData.radius.CopyTo(agentDataCopy.radius);
			agentData.height.CopyTo(agentDataCopy.height);
			agentData.desiredSpeed.CopyTo(agentDataCopy.desiredSpeed);
			agentData.maxSpeed.CopyTo(agentDataCopy.maxSpeed);
			agentData.agentTimeHorizon.CopyTo(agentDataCopy.agentTimeHorizon);
			agentData.obstacleTimeHorizon.CopyTo(agentDataCopy.obstacleTimeHorizon);
			agentData.maxNeighbours.CopyTo(agentDataCopy.maxNeighbours);
			agentData.layer.CopyTo(agentDataCopy.layer);
			agentData.collidesWith.CopyTo(agentDataCopy.collidesWith);
			agentData.flowFollowingStrength.CopyTo(agentDataCopy.flowFollowingStrength);
			agentData.position.CopyTo(agentDataCopy.position);
			agentData.collisionNormal.CopyTo(agentDataCopy.collisionNormal);
			agentData.priority.CopyTo(agentDataCopy.priority);
			agentData.debugDraw.CopyTo(agentDataCopy.debugDraw);
			agentData.targetPoint.CopyTo(agentDataCopy.targetPoint);
			agentData.manuallyControlled.CopyTo(agentDataCopy.manuallyControlled);
			agentData.movementPlane.CopyTo(agentDataCopy.movementPlane);

			bool debug = false;
			for (int i = startIndex; i < endIndex; i++) {
				// Manually controlled overrides the agent being locked.
				// If one for some reason uses them at the same time.
				agentDataCopy.locked[i] = agentData.locked[i] & !agentData.manuallyControlled[i];

				debug |= agentData.debugDraw[i];
			}
			anyDebugEnabled[0] = debug;
		}
	}

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	public struct JobRVOPreprocess : IJob {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public SimulatorBurst.AgentOutputData previousOutput;

		[WriteOnly]
		public SimulatorBurst.TemporaryAgentData temporaryAgentData;

		public int startIndex;
		public int endIndex;

		public void Execute () {
			for (int i = startIndex; i < endIndex; i++) {
				// Manually controlled overrides the agent being locked.
				// If one for some reason uses them at the same time.
				var locked = agentData.locked[i] & !agentData.manuallyControlled[i];

				if (locked) {
					temporaryAgentData.desiredTargetPointInVelocitySpace[i] = float2.zero;
					temporaryAgentData.desiredVelocity[i] = float3.zero;
					temporaryAgentData.currentVelocity[i] = float3.zero;
				} else {
					var desiredTargetPointInVelocitySpace = agentData.movementPlane[i].ToPlane(agentData.targetPoint[i] - agentData.position[i]);
					temporaryAgentData.desiredTargetPointInVelocitySpace[i] = desiredTargetPointInVelocitySpace;

					// Estimate our current velocity
					// This is necessary because other agents need to know
					// how this agent is moving to be able to avoid it
					var currentVelocity = math.normalizesafe(previousOutput.targetPoint[i] - agentData.position[i]) * previousOutput.speed[i];

					// Calculate the desired velocity from the point we want to reach
					temporaryAgentData.desiredVelocity[i] = agentData.movementPlane[i].ToWorld(math.normalizesafe(desiredTargetPointInVelocitySpace) * agentData.desiredSpeed[i], 0);

					var collisionNormal = math.normalizesafe(agentData.collisionNormal[i]);
					// Check if the velocity is going into the wall
					// If so: remove that component from the velocity
					// Note: if the collisionNormal is zero then the dot prodct will produce a zero as well and nothing will happen.
					float dot = math.dot(currentVelocity, collisionNormal);
					currentVelocity -= math.min(0, dot) * collisionNormal;
					temporaryAgentData.currentVelocity[i] = currentVelocity;
				}
			}
		}
	}

	/// <summary>
	/// Inspired by StarCraft 2's avoidance of locked units.
	/// See: http://www.gdcvault.com/play/1014514/AI-Navigation-It-s-Not
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHorizonAvoidancePhase1 : PathfindingCore.Jobs.IJobParallelForBatched {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public NativeArray<float2> desiredTargetPointInVelocitySpace;

		[ReadOnly]
		public NativeArray<int> neighbours;

		public SimulatorBurst.HorizonAgentData horizonAgentData;

		public CommandBuilder draw;

		public bool allowBoundsChecks { get { return true; } }

		/// <summary>
		/// Super simple bubble sort.
		/// TODO: This will be replaced by a better implementation from the Unity.Collections library when that is stable.
		/// </summary>
		static void Sort<T>(NativeSlice<T> arr, NativeSlice<float> keys) where T : struct {
			bool changed = true;

			while (changed) {
				changed = false;
				for (int i = 0; i < arr.Length - 1; i++) {
					if (keys[i] > keys[i+1]) {
						var tmp = keys[i];
						var tmp2 = arr[i];
						keys[i] = keys[i+1];
						keys[i+1] = tmp;
						arr[i] = arr[i+1];
						arr[i+1] = tmp2;
						changed = true;
					}
				}
			}
		}


		/// <summary>Calculates the shortest difference between two given angles given in radians.</summary>
		public static float DeltaAngle (float current, float target) {
			float num = Mathf.Repeat(target - current, math.PI*2);

			if (num > math.PI) {
				num -= math.PI*2;
			}
			return num;
		}

		public void Execute (int startIndex, int count) {
			NativeArray<float> angles = new NativeArray<float>(SimulatorBurst.MaxNeighbourCount*2, Allocator.Temp);
			NativeArray<int> deltas = new NativeArray<int>(SimulatorBurst.MaxNeighbourCount*2, Allocator.Temp);

			for (int i = startIndex; i < startIndex + count; i++) {
				if (agentData.locked[i] || agentData.manuallyControlled[i]) {
					horizonAgentData.horizonSide[i] = 0;
					horizonAgentData.horizonMinAngle[i] = 0;
					horizonAgentData.horizonMaxAngle[i] = 0;
					continue;
				}

				float minAngle = 0;
				float maxAngle = 0;

				float desiredAngle = math.atan2(desiredTargetPointInVelocitySpace[i].y, desiredTargetPointInVelocitySpace[i].x);

				int eventCount = 0;

				int inside = 0;

				float radius = agentData.radius[i];

				var position = agentData.position[i];
				var movementPlane = agentData.movementPlane[i];

				var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
				for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
					var other = agentNeighbours[j];
					if (!agentData.locked[other] && !agentData.manuallyControlled[other]) continue;

					var relativePosition = movementPlane.ToPlane(agentData.position[other] - position);
					float dist = math.length(relativePosition);

					float angle = math.atan2(relativePosition.y, relativePosition.x) - desiredAngle;
					float deltaAngle;

					var otherRadius = agentData.radius[other];
					if (dist < radius + otherRadius) {
						// Collision
						deltaAngle = math.PI * 0.49f;
					} else {
						// One degree
						const float AngleMargin = math.PI / 180f;
						deltaAngle = math.asin((radius + otherRadius)/dist) + AngleMargin;
					}

					float aMin = DeltaAngle(0, angle - deltaAngle);
					float aMax = aMin + DeltaAngle(aMin, angle + deltaAngle);

					if (aMin < 0 && aMax > 0) inside++;

					angles[eventCount] = aMin;
					deltas[eventCount] = 1;
					eventCount++;
					angles[eventCount] = aMax;
					deltas[eventCount] = -1;
					eventCount++;
				}

				// If no angle range includes angle 0 then we are already done
				if (inside == 0) {
					horizonAgentData.horizonSide[i] = 0;
					horizonAgentData.horizonMinAngle[i] = 0;
					horizonAgentData.horizonMaxAngle[i] = 0;
					continue;
				}

				// Sort the events by their angle in ascending order
				Sort(deltas.Slice(0, eventCount), angles.Slice(0, eventCount));

				// Find the first index for which the angle is positive
				int firstPositiveIndex = 0;
				for (; firstPositiveIndex < eventCount; firstPositiveIndex++) if (angles[firstPositiveIndex] > 0) break;

				// Walk in the positive direction from angle 0 until the end of the group of angle ranges that include angle 0
				int tmpInside = inside;
				int tmpIndex = firstPositiveIndex;
				for (; tmpIndex < eventCount; tmpIndex++) {
					tmpInside += deltas[tmpIndex];
					if (tmpInside == 0) break;
				}
				maxAngle = tmpIndex == eventCount ? math.PI : angles[tmpIndex];

				// Walk in the negative direction from angle 0 until the end of the group of angle ranges that include angle 0
				tmpInside = inside;
				tmpIndex = firstPositiveIndex - 1;
				for (; tmpIndex >= 0; tmpIndex--) {
					tmpInside -= deltas[tmpIndex];
					if (tmpInside == 0) break;
				}
				minAngle = tmpIndex == -1 ? -math.PI : angles[tmpIndex];

				//horizonBias = -(minAngle + maxAngle);

				// Indicates that a new side should be chosen. The "best" one will be chosen later.
				if (horizonAgentData.horizonSide[i] == 0) horizonAgentData.horizonSide[i] = 2;
				//else horizonBias = math.PI * horizonSide;

				horizonAgentData.horizonMinAngle[i] = minAngle + desiredAngle;
				horizonAgentData.horizonMaxAngle[i] = maxAngle + desiredAngle;
			}
		}
	}

	/// <summary>
	/// Inspired by StarCraft 2's avoidance of locked units.
	/// See: http://www.gdcvault.com/play/1014514/AI-Navigation-It-s-Not
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHorizonAvoidancePhase2 : PathfindingCore.Jobs.IJobParallelForBatched {
		[ReadOnly]
		public NativeArray<int> neighbours;
		public NativeArray<float3> desiredVelocity;
		public NativeArray<float2> desiredTargetPointInVelocitySpace;

		[ReadOnly]
		public NativeArray<SimpleMovementPlane> movementPlane;

		public SimulatorBurst.HorizonAgentData horizonAgentData;

		public bool allowBoundsChecks => false;

		public void Execute (int startIndex, int count) {
			for (int i = startIndex; i < startIndex + count; i++) {
				// Note: Assumes this code is run synchronous (i.e not included in the double buffering part)
				//offsetVelocity = (position - Position) / simulator.DeltaTime;

				if (horizonAgentData.horizonSide[i] == 0) {
					continue;
				}

				if (horizonAgentData.horizonSide[i] == 2) {
					float sum = 0;
					var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
					for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
						var other = agentNeighbours[j];
						var otherHorizonBias = -(horizonAgentData.horizonMinAngle[other] + horizonAgentData.horizonMaxAngle[other]);
						sum += otherHorizonBias;
					}
					var horizonBias = -(horizonAgentData.horizonMinAngle[i] + horizonAgentData.horizonMaxAngle[i]);
					sum += horizonBias;

					horizonAgentData.horizonSide[i] = sum < 0 ? -1 : 1;
				}

				float bestAngle = horizonAgentData.horizonSide[i] < 0 ? horizonAgentData.horizonMinAngle[i] : horizonAgentData.horizonMaxAngle[i];
				float2 desiredDirection;
				math.sincos(bestAngle, out desiredDirection.y, out desiredDirection.x);
				desiredVelocity[i] = movementPlane[i].ToWorld(math.length(desiredVelocity[i]) * desiredDirection, 0);
				desiredTargetPointInVelocitySpace[i] = math.length(desiredTargetPointInVelocitySpace[i]) * desiredDirection;
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHardCollisions<MovementPlaneWrapper> : PathfindingCore.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;
		[ReadOnly]
		public NativeArray<int> neighbours;
		[WriteOnly]
		public NativeArray<float2> collisionVelocityOffsets;

		public float deltaTime;
		public bool enabled;

		/// <summary>
		/// How aggressively hard collisions are resolved.
		/// Should be a value between 0 and 1.
		/// </summary>
		const float CollisionStrength = 0.8f;

		public bool allowBoundsChecks => false;

		public void Execute (int startIndex, int count) {
			if (!enabled) {
				for (int i = startIndex; i < startIndex + count; i++) {
					collisionVelocityOffsets[i] = float2.zero;
				}
				return;
			}

			for (int i = startIndex; i < startIndex + count; i++) {
				if (agentData.locked[i]) {
					collisionVelocityOffsets[i] = float2.zero;
					continue;
				}

				var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
				var radius = agentData.radius[i];
				var totalOffset = float2.zero;
				float totalWeight = 0;

				var position = agentData.position[i];
				var movementPlane = new MovementPlaneWrapper();
				movementPlane.Set(agentData.movementPlane[i]);

				for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
					var other = agentNeighbours[j];
					var relativePosition = movementPlane.ToPlane(position - agentData.position[other]);

					var dirSqrLength = math.lengthsq(relativePosition);
					var combinedRadius = agentData.radius[other] + radius;
					if (dirSqrLength < combinedRadius*combinedRadius && dirSqrLength > 0.00000001f) {
						// Collision
						var dirLength = math.sqrt(dirSqrLength);
						var normalizedDir = relativePosition * (1.0f / dirLength);

						// Overlap amount
						var weight = combinedRadius - dirLength;

						// Position offset required to make the agents not collide anymore
						var offset = normalizedDir * weight;
						// In a later step a weighted average will be taken so that the average offset is extracted
						var weightedOffset = offset * weight;

						totalOffset += weightedOffset;
						totalWeight += weight;
					}
				}

				var offsetVelocity = totalOffset * (1.0f / (0.0001f + totalWeight));
				offsetVelocity *= (CollisionStrength * 0.5f) / deltaTime;

				collisionVelocityOffsets[i] = offsetVelocity;
			}
		}
	}

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public struct JobRVOCalculateNeighbours<MovementPlaneWrapper> : PathfindingCore.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public RVOQuadtreeBurst quadtree;

		public NativeArray<int> outNeighbours;

		[WriteOnly]
		public SimulatorBurst.AgentOutputData output;

		public bool allowBoundsChecks { get { return false; } }

		public void Execute (int startIndex, int count) {
			NativeArray<float> neighbourDistances = new NativeArray<float>(SimulatorBurst.MaxNeighbourCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int i = startIndex; i < startIndex + count; i++) {
				CalculateNeighbours(i, outNeighbours, neighbourDistances);
			}
		}

		void CalculateNeighbours (int agentIndex, NativeArray<int> neighbours, NativeArray<float> neighbourDistances) {
			int maxNeighbourCount = math.min(SimulatorBurst.MaxNeighbourCount, agentData.maxNeighbours[agentIndex]);
			// Write the output starting at this index in the neighbours array
			var outputIndex = agentIndex * SimulatorBurst.MaxNeighbourCount;

			quadtree.QueryKNearest(new RVOQuadtreeBurst.QuadtreeQuery {
				position = agentData.position[agentIndex],
				speed = agentData.maxSpeed[agentIndex],
				agentRadius = agentData.radius[agentIndex],
				timeHorizon = agentData.agentTimeHorizon[agentIndex],
				outputStartIndex = outputIndex,
				maxCount = maxNeighbourCount,
				result = neighbours,
				resultDistances = neighbourDistances,
			});

			int numNeighbours = 0;
			while (numNeighbours < maxNeighbourCount && math.isfinite(neighbourDistances[numNeighbours])) numNeighbours++;
			output.numNeighbours[agentIndex] = numNeighbours;

			MovementPlaneWrapper movementPlane = default;
			movementPlane.Set(agentData.movementPlane[agentIndex]);
			movementPlane.ToPlane(agentData.position[agentIndex], out float localElevation);

			// Filter out invalid neighbours
			for (int i = 0; i < numNeighbours; i++) {
				int otherIndex = neighbours[outputIndex + i];
				// Interval along the y axis in which the agents overlap
				movementPlane.ToPlane(agentData.position[otherIndex], out float otherElevation);
				float maxY = math.min(localElevation + agentData.height[agentIndex], otherElevation + agentData.height[otherIndex]);
				float minY = math.max(localElevation, otherElevation);

				// The agents cannot collide if they are on different y-levels.
				// Also do not avoid the agent itself.
				// Apply the layer masks for agents.
				// Use binary OR to reduce branching.
				if ((maxY < minY) | (otherIndex == agentIndex) | (((int)agentData.collidesWith[agentIndex] & (int)agentData.layer[otherIndex]) == 0)) {
					numNeighbours--;
					neighbours[outputIndex + i] = neighbours[outputIndex + numNeighbours];
					i--;
				}
			}

			// Add a token indicating the size of the neighbours list
			if (numNeighbours < SimulatorBurst.MaxNeighbourCount) neighbours[outputIndex + numNeighbours] = -1;
		}
	}

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	[Il2CppSetOption(Option.DivideByZeroChecks, false)]
	[Il2CppSetOption(Option.NullChecks, false)]
	public struct JobRVO<MovementPlaneWrapper> : PathfindingCore.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public SimulatorBurst.TemporaryAgentData temporaryAgentData;

		[WriteOnly]
		public SimulatorBurst.AgentOutputData output;

		/// <summary>Should be in the range [0,1]</summary>
		public float collisionStrength;

		public float deltaTime;
		public float symmetryBreakingBias;

		public bool allowBoundsChecks { get { return true; } }

		const int MaxObstacleCount = 50;

		const float DesiredVelocityWeight = 0.1f;

		public CommandBuilder draw;

		public void Execute (int startIndex, int batchSize) {
			ExecuteORCA(startIndex, batchSize);
			//ExecuteSampled(startIndex, batchSize);
		}

		public void ExecuteSampled (int startIndex, int batchSize) {
			int endIndex = startIndex + batchSize;

			NativeArray<VelocityObstacle> velocityObstacles = new NativeArray<VelocityObstacle>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int agentIndex = startIndex; agentIndex < endIndex; agentIndex++) {
				if (agentData.manuallyControlled[agentIndex]) {
					// TODO
					continue;
				}

				var position = agentData.position[agentIndex];

				if (agentData.locked[agentIndex]) {
					output.speed[agentIndex] = 0;
					output.targetPoint[agentIndex] = position;
					continue;
				}

				var neighbours = temporaryAgentData.neighbours.Slice(agentIndex*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);

				int numVOs = GenerateNeighbourAgentVOs(agentIndex, velocityObstacles, neighbours);
				//GenerateObstacleVOs(velocityObstacles);

				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(agentData.movementPlane[agentIndex]);
				var localDesiredVelocity = movementPlane.ToPlane(temporaryAgentData.desiredVelocity[agentIndex]);
				var desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace[agentIndex];

				bool insideAnyVO = BiasDesiredVelocity(velocityObstacles.Slice(0, numVOs), ref localDesiredVelocity, ref desiredTargetPointInVelocitySpace, symmetryBreakingBias);

				if (!insideAnyVO && math.all(math.abs(temporaryAgentData.collisionVelocityOffsets[agentIndex]) < 0.001f)) {
					// Desired velocity can be used directly since it was not inside any velocity obstacle.
					// No need to run optimizer because this will be the global minima.
					// This is also a special case in which we can set the
					// calculated target point to the desired target point
					// instead of calculating a point based on a calculated velocity
					// which is an important difference when the agent is very close
					// to the target point
					// TODO: Not actually guaranteed to be global minima if desiredTargetPointInVelocitySpace.magnitude < desiredSpeed
					// maybe do something different here?
					output.targetPoint[agentIndex] = position + movementPlane.ToWorld(desiredTargetPointInVelocitySpace);
					output.speed[agentIndex] = agentData.desiredSpeed[agentIndex];
					//if (DebugDraw) Draw.CrossXZ(FromXZ(calculatedTargetPoint), Color.white);
					continue;
				}

				float2 result = GradientDescent(agentIndex, velocityObstacles.Slice(0, numVOs), movementPlane.ToPlane(temporaryAgentData.currentVelocity[agentIndex]), localDesiredVelocity, localDesiredVelocity);

				//if (DebugDraw) Draw.CrossXZ(FromXZ(result+position), Color.white);
				//Debug.DrawRay (To3D (position), To3D (result));

				// Make the agent move to avoid intersecting other agents (hard collisions)
				result += temporaryAgentData.collisionVelocityOffsets[agentIndex];

				output.targetPoint[agentIndex] = position + movementPlane.ToWorld(result, 0);
				output.speed[agentIndex] = math.min(math.length(result), agentData.maxSpeed[agentIndex]);
			}

			velocityObstacles.Dispose();
		}

		public void ExecuteORCA (int startIndex, int batchSize) {
			int endIndex = startIndex + batchSize;

			NativeArray<ORCALine> orcaLines = new NativeArray<ORCALine>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<ORCALine> scratchBuffer = new NativeArray<ORCALine>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int agentIndex = startIndex; agentIndex < endIndex; agentIndex++) {
				if (agentData.manuallyControlled[agentIndex]) {
					// TODO
					continue;
				}

				var position = agentData.position[agentIndex];

				if (agentData.locked[agentIndex]) {
					output.speed[agentIndex] = 0;
					output.targetPoint[agentIndex] = position;
					continue;
				}

				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(agentData.movementPlane[agentIndex]);

				var neighbours = temporaryAgentData.neighbours.Slice(agentIndex*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);

				//int numVOs = GenerateNeighbourAgentVOs(agentIndex, velocityObstacles, neighbours);
				//GenerateObstacleVOs(velocityObstacles);

				float inverseAgentTimeHorizon = 1.0f/agentData.agentTimeHorizon[agentIndex];
				float priority = agentData.priority[agentIndex];
				int numLines = 0;

				// The RVO algorithm assumes we will continue to
				// move in roughly the same direction
				float2 optimalVelocity = movementPlane.ToPlane(temporaryAgentData.currentVelocity[agentIndex]);
				var localPosition = movementPlane.ToPlane(position);

				for (; numLines < neighbours.Length; numLines++) {
					int otherIndex = neighbours[numLines];
					// Indicates that there are no more neighbours (see JobRVOCalculateNeighbours)
					if (otherIndex == -1) break;

					var otherPosition = agentData.position[otherIndex];
					var relativePosition = movementPlane.ToPlane(otherPosition - position);
					float totalRadius = agentData.radius[agentIndex] + agentData.radius[otherIndex];

					// TODO: Remove branches to possibly vectorize
					float avoidanceStrength;
					if (agentData.locked[otherIndex] || agentData.manuallyControlled[otherIndex]) {
						avoidanceStrength = 1;
					} else if (agentData.priority[otherIndex] > 0.00001f || priority > 0.00001f) {
						avoidanceStrength = agentData.priority[otherIndex] / (priority + agentData.priority[otherIndex]);
					} else {
						// Both this agent's priority and the other agent's priority is zero or negative
						// Assume they have the same priority
						avoidanceStrength = 0.5f;
					}

					// We assume that the other agent will continue to move with roughly the same velocity if the priorities for the agents are similar.
					// If the other agent has a higher priority than this agent (avoidanceStrength > 0.5) then we will assume it will move more along its
					// desired velocity. This will have the effect of other agents trying to clear a path for where a high priority agent wants to go.
					// If this is not done then even high priority agents can get stuck when it is really crowded and they have had to slow down.
					float2 otherOptimalVelocity = movementPlane.ToPlane(math.lerp(temporaryAgentData.currentVelocity[otherIndex], temporaryAgentData.desiredVelocity[otherIndex], 2*avoidanceStrength - 1));

					if (agentData.flowFollowingStrength[otherIndex] > 0) {
						// When flow following strength is 1 the component of the other agent's velocity that is in the direction of this agent is removed.
						// That is, we pretend that the other agent does not move towards this agent at all.
						// This will make it impossible for the other agent to "push" this agent away.
						var relativeDir = math.normalizesafe(relativePosition);
						otherOptimalVelocity -= relativeDir * (agentData.flowFollowingStrength[otherIndex] * math.min(0, math.dot(otherOptimalVelocity, relativeDir)));
					}

					orcaLines[numLines] = new ORCALine(localPosition, relativePosition, optimalVelocity, otherOptimalVelocity, totalRadius, 0.1f, inverseAgentTimeHorizon);

					if (agentData.debugDraw[agentIndex]) {
						draw.Line(FromXZ(localPosition + orcaLines[numLines].point - orcaLines[numLines].direction*10), FromXZ(localPosition + orcaLines[numLines].point + orcaLines[numLines].direction*10), Color.magenta);
						float2 voBoundingOrigin = movementPlane.ToPlane(agentData.position[otherIndex]) - localPosition;
						var voCenter = math.lerp(optimalVelocity, otherOptimalVelocity, 0.5f);
						DrawVO(draw, localPosition + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, localPosition + voCenter);
					}
				}

				var desiredVelocity = movementPlane.ToPlane(temporaryAgentData.desiredVelocity[agentIndex]);
				var desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace[agentIndex];

				bool insideAnyVO = BiasDesiredVelocity(orcaLines, numLines, ref desiredVelocity, ref desiredTargetPointInVelocitySpace, symmetryBreakingBias);

				if (!insideAnyVO && math.all(math.abs(temporaryAgentData.collisionVelocityOffsets[agentIndex]) < 0.001f)) {
					// Desired velocity can be used directly since it was not inside any velocity obstacle.
					// No need to run optimizer because this will be the global minima.
					// This is also a special case in which we can set the
					// calculated target point to the desired target point
					// instead of calculating a point based on a calculated velocity
					// which is an important difference when the agent is very close
					// to the target point
					// TODO: Not actually guaranteed to be global minima if desiredTargetPointInVelocitySpace.magnitude < desiredSpeed
					// maybe do something different here?
					output.targetPoint[agentIndex] = position + movementPlane.ToWorld(desiredTargetPointInVelocitySpace, 0);
					output.speed[agentIndex] = agentData.desiredSpeed[agentIndex];
					//if (DebugDraw) Draw.CrossXZ(FromXZ(calculatedTargetPoint), Color.white);
				} else {
					var lin = LinearProgram2(orcaLines, numLines, agentData.maxSpeed[agentIndex], desiredVelocity, false);

					float2 newVelocity;
					if (lin.firstFailedLineIndex < numLines) {
						newVelocity = lin.velocity;
						LinearProgram3(orcaLines, numLines, 0, lin.firstFailedLineIndex, agentData.maxSpeed[agentIndex], ref newVelocity, scratchBuffer);
					} else {
						newVelocity = lin.velocity;
					}

					//if (DebugDraw) Draw.CrossXZ(FromXZ(result+position), Color.white);
					//Debug.DrawRay (To3D (position), To3D (result));

					// Make the agent move to avoid intersecting other agents (hard collisions)
					newVelocity += temporaryAgentData.collisionVelocityOffsets[agentIndex];

					output.targetPoint[agentIndex] = position + movementPlane.ToWorld(newVelocity, 0);
					output.speed[agentIndex] = math.min(math.length(newVelocity), agentData.maxSpeed[agentIndex]);
				}
			}

			orcaLines.Dispose();
		}

		static float det (float2 vector1, float2 vector2) {
			return vector1.x * vector2.y - vector1.y * vector2.x;
		}

		struct ORCALine {
			public float2 point;
			public float2 direction;

			public ORCALine(float2 position, float2 relativePosition, float2 velocity, float2 otherVelocity, float combinedRadius, float timeStep, float invTimeHorizon) {
				var relativeVelocity = velocity - otherVelocity;
				float combinedRadiusSq = combinedRadius*combinedRadius;
				float distSq = math.lengthsq(relativePosition);

				if (distSq > combinedRadiusSq) {
					combinedRadius *= 1.001f;
					// No collision

					// Vector from cutoff center to relative velocity
					var w = relativeVelocity - invTimeHorizon * relativePosition;
					var wLengthSq = math.lengthsq(w);

					float dot1 = math.dot(w, relativePosition);

					if (dot1 < 0.0f && dot1*dot1 > combinedRadiusSq * wLengthSq) {
						// Project on cut-off circle
						float wLength = math.sqrt(wLengthSq);
						var normalizedW = w / wLength;

						direction = new float2(normalizedW.y, -normalizedW.x);
						var u = (combinedRadius * invTimeHorizon - wLength) * normalizedW;
						point = velocity + 0.5f * u;

						float time = (math.sqrt(distSq) - combinedRadius) / math.max(combinedRadius, math.length(relativeVelocity));
						point += math.sqrt(distSq) * math.normalize(new float2(direction.y, -direction.x)) * (1.0f - math.unlerp(1.0f, 0.0f, time*0.5f));
					} else {
						// Project on legs
						// Distance from the agent to the point where the "legs" start on the other agent
						float legDistance = math.sqrt(distSq - combinedRadiusSq);

						if (det(relativePosition, w) > 0.0f) {
							// Project on left leg
							direction = (relativePosition * legDistance + new float2(-relativePosition.y, relativePosition.x) * combinedRadius) / distSq;
							//direction = new float2(relativePosition.x * legDistance - relativePosition.y * combinedRadius, relativePosition.x * combinedRadius + relativePosition.y * legDistance) / distSq;
						} else {
							// Project on right leg
							direction = (-relativePosition * legDistance + new float2(-relativePosition.y, relativePosition.x) * combinedRadius) / distSq;
							//direction = -new float2(relativePosition.x * legDistance + relativePosition.y * combinedRadius, -relativePosition.x * combinedRadius + relativePosition.y * legDistance) / distSq;
						}

						float dot2 = math.dot(relativeVelocity, direction);
						var u = dot2 * direction - relativeVelocity;
						point = velocity + 0.5f * u;
						float time = (math.sqrt(distSq) - combinedRadius) / math.max(combinedRadius, math.length(relativeVelocity));
						point += math.sqrt(distSq) * math.normalize(new float2(direction.y, -direction.x)) * (1.0f - math.unlerp(1.0f, 0.0f, time*0.5f));
						//point = math.lerp(velocity, otherVelocity, 0.5f) + 0.5f * dot2 * direction;
					}
				} else {
					float invTimeStep = 1.0f / timeStep;
					var u = math.normalizesafe(relativePosition) * (math.length(relativePosition) - combinedRadius - 0.001f) * 0.3f * invTimeStep;
					direction = math.normalize(new float2(u.y, -u.x));
					point = math.lerp(velocity, otherVelocity, 0.5f) + u * 0.5f;


					// Original code, the above is a version which works better
					// Collision
					// Project on cut-off circle of timeStep
					//float invTimeStep = 1.0f / timeStep;
					// Vector from cutoff center to relative velocity
					//float2 w = relativeVelocity - invTimeStep * relativePosition;
					//float wLength = math.length(w);
					//float2 unitW = w / wLength;
					//direction = new float2(unitW.y, -unitW.x);
					//var u = (combinedRadius * invTimeStep - wLength) * unitW;
					//point = velocity + 0.5f * u;
				}
			}
		}

		/// <summary>
		/// Bias towards the right side of agents.
		/// Rotate desiredVelocity at most [value] number of radians. 1 radian ≈ 57°
		/// This breaks up symmetries.
		///
		/// The desired velocity will only be rotated if it is inside a velocity obstacle (VO).
		/// If it is inside one, it will not be rotated further than to the edge of it
		///
		/// The targetPointInVelocitySpace will be rotated by the same amount as the desired velocity
		///
		/// Returns: True if the desired velocity was inside any VO
		/// </summary>
		bool BiasDesiredVelocity (NativeArray<ORCALine> lines, int numLines, ref float2 desiredVelocity, ref float2 targetPointInVelocitySpace, float maxBiasRadians) {
			float maxDistance = 0.0f;

			for (int i = 0; i < numLines; i++) {
				var distance = det(lines[i].direction, lines[i].point - desiredVelocity) / math.length(lines[i].direction);
				maxDistance = math.max(maxDistance, distance);
			}

			if (maxDistance == 0.0f) return false;

			var desiredVelocityMagn = math.length(desiredVelocity);

			// Avoid division by zero below
			if (desiredVelocityMagn < 0.001f) return true;

			// Rotate the desired velocity clockwise (to the right) at most maxBiasRadians number of radians
			// Assuming maxBiasRadians is small, we can just move it instead and it will give approximately the same effect
			// See https://en.wikipedia.org/wiki/Small-angle_approximation
			var angle = math.min(maxBiasRadians, maxDistance / desiredVelocityMagn);
			desiredVelocity += new float2(desiredVelocity.y, -desiredVelocity.x) * angle;
			targetPointInVelocitySpace += new float2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;
			return true;
		}

		bool LinearProgram1 (NativeArray<ORCALine> lines, int lineIndex, float radius, float2 optimalVelocity, bool directionOpt, ref float2 result) {
			var line = lines[lineIndex];
			float dot = math.dot(line.point, line.direction);
			float discriminant = dot*dot + radius*radius - math.lengthsq(line.point);

			if (discriminant < 0.0f) {
				// Max speed circle fully invalidates line lineIndex
				return false;
			}

			var sqrtDiscriminant = math.sqrt(discriminant);
			var tLeft = -dot - sqrtDiscriminant;
			float tRight = -dot + sqrtDiscriminant;

			for (int i = 0; i < lineIndex; i++) {
				float denominator = det(line.direction, lines[i].direction);
				float numerator = det(lines[i].direction, line.point - lines[i].point);

				if (math.abs(denominator) < 0.0001f) {
					// The two lines are almost parallel
					if (numerator < 0.0f) return false;
					else continue;
				}

				float t = numerator / denominator;

				if (denominator >= 0.0f) {
					// Line i bounds the line on the right
					tRight = math.min(tRight, t);
				} else {
					// Line i bounds the line on the left
					tLeft = math.max(tLeft, t);
				}

				if (tLeft > tRight) {
					return false;
				}
			}

			if (directionOpt) {
				// Optimize direction
				if (math.dot(optimalVelocity, line.direction) > 0.0f) {
					// Take right extreme
					result = line.point + tRight * line.direction;
				} else {
					// Take left extreme
					result = line.point + tLeft * line.direction;
				}
			} else {
				// Optimize closest point
				float t = math.dot(line.direction, optimalVelocity - line.point);
				result = line.point + math.clamp(t, tLeft, tRight) * line.direction;
				//if (t < tLeft) {
				//	result = line.point + tLeft * line.direction;
				//} else if (t > tRight) {
				//	result = line.point + tRight * line.direction;
				//} else {
				//	result = line.point + t * line.direction;
				//}
			}
			return true;
		}


		struct LinearProgram2Output {
			public float2 velocity;
			public int firstFailedLineIndex;
		}

		LinearProgram2Output LinearProgram2 (NativeArray<ORCALine> lines, int numLines, float radius, float2 optimalVelocity, bool directionOpt) {
			float2 result;

			if (directionOpt) {
				// Optimize direction. Note that the optimization velocity is of unit length in this case
				result = optimalVelocity * radius;
			} else if (math.lengthsq(optimalVelocity) > radius*radius) {
				// Optimize closest point and outside circle
				result = math.normalize(optimalVelocity) * radius;
			} else {
				// Optimize closest point and inside circle
				result = optimalVelocity;
			}

			for (int i = 0; i < numLines; i++) {
				if (det(lines[i].direction, lines[i].point - result) > 0.0f) {
					// Result does not satisfy constraint i. Compute new optimal result
					var tempResult = result;
					if (!LinearProgram1(lines, i, radius, optimalVelocity, directionOpt, ref result)) {
						return new LinearProgram2Output {
								   velocity = tempResult,
								   firstFailedLineIndex = i,
						};
					}
				}
			}

			return new LinearProgram2Output {
					   velocity = result,
					   firstFailedLineIndex = numLines,
			};
		}

		void LinearProgram3 (NativeArray<ORCALine> lines, int numLines, int numFixedLines, int beginLine, float radius, ref float2 result, NativeArray<ORCALine> scratchBuffer) {
			float distance = 0.0f;

			for (int i = beginLine; i < numLines; i++) {
				if (det(lines[i].direction, lines[i].point - result) > distance) {
					NativeArray<ORCALine> projectedLines = scratchBuffer;
					int numProjectedLines = 0;
					for (int j = 0; j < i; j++) {
						float determinant = det(lines[i].direction, lines[j].direction);
						if (math.abs(determinant) < 0.001f) {
							// Lines i and j are parallel
							if (math.dot(lines[i].direction, lines[j].direction) > 0.0f) {
								// Line i and j point in the same direction
								continue;
							} else {
								// Line i and j point in the opposite direction
								projectedLines[numProjectedLines] = new ORCALine {
									point = 0.5f * (lines[i].point + lines[j].point),
									direction = math.normalize(lines[j].direction - lines[i].direction),
								};
								numProjectedLines++;
							}
						} else {
							projectedLines[numProjectedLines] = new ORCALine {
								point = lines[i].point + (det(lines[j].direction, lines[i].point - lines[j].point) / determinant) * lines[i].direction,
								direction = math.normalize(lines[j].direction - lines[i].direction),
							};
							numProjectedLines++;
						}
					}

					var lin = LinearProgram2(projectedLines, numProjectedLines, radius, new float2(-lines[i].direction.y, lines[i].direction.x), true);
					if (lin.firstFailedLineIndex < numProjectedLines) {
						// This should in principle not happen.  The result is by definition
						// already in the feasible region of this linear program. If it fails,
						// it is due to small floating point error, and the current result is
						// kept.
					} else {
						result = lin.velocity;
					}

					distance = det(lines[i].direction, lines[i].point - result);
					projectedLines.Dispose();
				}
			}
		}




		/// <summary>(x, 0, y)</summary>
		static Vector3 FromXZ (float2 p) {
			return new Vector3(p.x, 0, p.y);
		}

		static void DrawVO (CommandBuilder draw, float2 circleCenter, float radius, float2 origin) {
#if UNITY_EDITOR
			float alpha = math.atan2((origin - circleCenter).y, (origin - circleCenter).x);
			float gamma = radius/math.length(origin-circleCenter);
			float delta = gamma <= 1.0f ? math.abs(math.acos(gamma)) : 0;

			draw.CircleXZ(FromXZ(circleCenter), radius, alpha-delta, alpha+delta, Color.black);
			float2 p1 = new float2(math.cos(alpha-delta), math.sin(alpha-delta)) * radius;
			float2 p2 = new float2(math.cos(alpha+delta), math.sin(alpha+delta)) * radius;

			float2 p1t = -new float2(-p1.y, p1.x);
			float2 p2t = new float2(-p2.y, p2.x);
			p1 += circleCenter;
			p2 += circleCenter;

			draw.Ray(FromXZ(p1), FromXZ(p1t).normalized*100, Color.black);
			draw.Ray(FromXZ(p2), FromXZ(p2t).normalized*100, Color.black);
#endif
		}

		int GenerateNeighbourAgentVOs (int agentIndex,  NativeArray<VelocityObstacle> velocityObstacles, NativeSlice<int> neighbours) {
			float inverseAgentTimeHorizon = 1.0f/agentData.agentTimeHorizon[agentIndex];

			// The RVO algorithm assumes we will continue to
			// move in roughly the same direction
			MovementPlaneWrapper movementPlane = default;

			movementPlane.Set(agentData.movementPlane[agentIndex]);
			float2 optimalVelocity = movementPlane.ToPlane(temporaryAgentData.currentVelocity[agentIndex]);
			var position = agentData.position[agentIndex];
			float priority = agentData.priority[agentIndex];
			var localPosition = movementPlane.ToPlane(position);
			int voIndex = 0;

			for (; voIndex < neighbours.Length; voIndex++) {
				int otherIndex = neighbours[voIndex];
				// Indicates that there are no more neighbours (see JobRVOCalculateNeighbours)
				if (otherIndex == -1) break;

				float totalRadius = agentData.radius[agentIndex] + agentData.radius[otherIndex];

				// Describes a circle on the border of the VO
				float2 voBoundingOrigin = movementPlane.ToPlane(agentData.position[otherIndex] - position);

				// TODO: Remove branches to possibly vectorize
				float avoidanceStrength;
				if (agentData.locked[otherIndex] || agentData.manuallyControlled[otherIndex]) {
					avoidanceStrength = 1;
				} else if (agentData.priority[otherIndex] > 0.00001f || priority > 0.00001f) {
					avoidanceStrength = agentData.priority[otherIndex] / (priority + agentData.priority[otherIndex]);
				} else {
					// Both this agent's priority and the other agent's priority is zero or negative
					// Assume they have the same priority
					avoidanceStrength = 0.5f;
				}

				// We assume that the other agent will continue to move with roughly the same velocity if the priorities for the agents are similar.
				// If the other agent has a higher priority than this agent (avoidanceStrength > 0.5) then we will assume it will move more along its
				// desired velocity. This will have the effect of other agents trying to clear a path for where a high priority agent wants to go.
				// If this is not done then even high priority agents can get stuck when it is really crowded and they have had to slow down.
				float2 otherOptimalVelocity = movementPlane.ToPlane(math.lerp(temporaryAgentData.currentVelocity[otherIndex], temporaryAgentData.desiredVelocity[otherIndex], 2*avoidanceStrength - 1));

				if (agentData.flowFollowingStrength[otherIndex] > 0) {
					// When flow following strength is 1 the component of the other agent's velocity that is in the direction of this agent is removed.
					// That is, we pretend that the other agent does not move towards this agent at all.
					// This will make it impossible for the other agent to "push" this agent away.
					var relativePosition = localPosition - movementPlane.ToPlane(agentData.position[otherIndex]);
					var relativeDir = math.normalizesafe(relativePosition);
					otherOptimalVelocity -= relativeDir * (agentData.flowFollowingStrength[otherIndex] * math.max(0, math.dot(otherOptimalVelocity, relativeDir)));
				}

				var voCenter = math.lerp(optimalVelocity, otherOptimalVelocity, avoidanceStrength);

				velocityObstacles[voIndex] = new VelocityObstacle(voBoundingOrigin, voCenter, totalRadius, inverseAgentTimeHorizon, 1 / deltaTime);
				if (agentData.debugDraw[agentIndex]) {
					DrawVO(draw, localPosition + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, localPosition + voCenter);
					velocityObstacles[voIndex].Draw(draw, voBoundingOrigin * inverseAgentTimeHorizon + voCenter, localPosition);
				}
			}

			return voIndex;
		}

		/// <summary>
		/// Bias towards the right side of agents.
		/// Rotate desiredVelocity at most [value] number of radians. 1 radian ≈ 57°
		/// This breaks up symmetries.
		///
		/// The desired velocity will only be rotated if it is inside a velocity obstacle (VO).
		/// If it is inside one, it will not be rotated further than to the edge of it
		///
		/// The targetPointInVelocitySpace will be rotated by the same amount as the desired velocity
		///
		/// Returns: True if the desired velocity was inside any VO
		/// </summary>
		static bool BiasDesiredVelocity (NativeSlice<VelocityObstacle> vos, ref float2 desiredVelocity, ref float2 targetPointInVelocitySpace, float maxBiasRadians) {
			var maxValue = 0f;

			for (int i = 0; i < vos.Length; i++) {
				float value;
				// The value is approximately the distance to the edge of the VO
				// so taking the maximum will give us the distance to the edge of the VO
				// which the desired velocity is furthest inside
				vos[i].Gradient(desiredVelocity, out value);
				maxValue = math.max(maxValue, value);
			}

			// Check if the agent was inside any VO
			var inside = maxValue > 0;

			var desiredVelocityMagn = math.length(desiredVelocity);

			// Avoid division by zero below
			if (desiredVelocityMagn < 0.001f) {
				return inside;
			}

			// Rotate the desired velocity clockwise (to the right) at most maxBiasRadians number of radians
			// Assuming maxBiasRadians is small, we can just move it instead and it will give approximately the same effect
			// See https://en.wikipedia.org/wiki/Small-angle_approximation
			var angle = math.min(maxBiasRadians, maxValue / desiredVelocityMagn);
			desiredVelocity += new float2(desiredVelocity.y, -desiredVelocity.x) * angle;
			targetPointInVelocitySpace += new float2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;
			return inside;
		}


		float2 GradientDescent (int agentIndex, NativeSlice<VelocityObstacle> vos, float2 sampleAround1, float2 sampleAround2, float2 desiredVelocity) {
			// Pick a reasonable initial step size
			float stepSize = math.max(agentData.radius[agentIndex], 0.2f * agentData.desiredSpeed[agentIndex]);
			var minima1 = Trace(agentIndex, vos, sampleAround1, out float score1, desiredVelocity, 10, stepSize);
			var minima2 = Trace(agentIndex, vos, sampleAround2, out float score2, desiredVelocity, 10, stepSize);

			var sampleAround3 = score1 < score2 ? minima1 : minima2;
			var minima3 = Trace(agentIndex, vos, sampleAround3, out float score3, desiredVelocity, 30, stepSize * 0.5f);

			//if (agentData.debugDraw[agentIndex]) Draw.CrossXZ(FromXZ(minima1 + agentData.position[agentIndex]), Color.magenta, 0.5f);
			//if (agentData.debugDraw[agentIndex]) Draw.CrossXZ(FromXZ(minima2 + agentData.position[agentIndex]), Color.magenta, 0.5f);
			//if (agentData.debugDraw[agentIndex]) Draw.CrossXZ(FromXZ(minima3 + agentData.position[agentIndex]), Color.cyan, 0.5f);
			return minima3;
		}

		static Color Rainbow (float v) {
			Color c = new Color(v, 0, 0);

			if (c.r > 1) {
				c.g = c.r - 1; c.r = 1;
			}
			if (c.g > 1) {
				c.b = c.g - 1; c.g = 1;
			}
			return c;
		}

		void DebugTrace (int agentIndex, float2 prev, float2 current, int stepIndex) {
			if (agentData.debugDraw[agentIndex]) draw.Line(FromXZ(prev) + (Vector3)agentData.position[agentIndex], FromXZ(current) + (Vector3)agentData.position[agentIndex], Rainbow(stepIndex*0.1f) * new Color(1, 1, 1, 1f));
		}


		/// <summary>
		/// Traces the vector field constructed out of the velocity obstacles.
		/// Returns the position which gives the minimum score (approximately).
		///
		/// See: https://en.wikipedia.org/wiki/Gradient_descent
		/// </summary>
		float2 Trace (int agentIndex, NativeSlice<VelocityObstacle> vos, float2 p, out float score, float2 desiredVelocity, int iterations, float stepSize) {
			float bestScore = float.PositiveInfinity;
			float2 bestP = p;

			const int MaxIterations = 50;
			const float StepSizeDecrease = 1.0f/MaxIterations;
			const float Momentum = 0.5f;

			float2 momentumVector = float2.zero;

			for (int s = 0; s < iterations; s++) {
				float step = 1.0f - s*StepSizeDecrease;
				step = (step*step) * stepSize;

				var gradient = EvaluateGradient(agentIndex, vos, p, out float value, desiredVelocity);

				if (value < bestScore) {
					bestScore = value;
					bestP = p;
				}

				// TODO: Add cutoff for performance

				gradient = math.normalizesafe(gradient);

				float2 prev = p;

				// Momentum
				gradient *= step;
				momentumVector = math.lerp(gradient, momentumVector, Momentum);
				p += momentumVector;
				DebugTrace(agentIndex, prev, p, s);
			}

			score = bestScore;
			return bestP;
		}

		/// <summary>Evaluate gradient and value of the cost function at velocity p</summary>
		float2 EvaluateGradient (int agentIndex, NativeSlice<VelocityObstacle> vos, float2 p, out float value, float2 desiredVelocity) {
			float2 gradient = float2.zero;

			value = 0;

			// Avoid other agents
			for (int i = 0; i < vos.Length; i++) {
				var voGradient = vos[i].ScaledGradient(p, out float voValue);
				if (voValue > value) {
					value = voValue;
					gradient = voGradient;
				}
			}

			float desiredSpeed = agentData.desiredSpeed[agentIndex];
			float maxSpeed = agentData.maxSpeed[agentIndex];

			// Move closer to the desired velocity
			var dirToDesiredVelocity = desiredVelocity - p;
			var distToDesiredVelocity = math.length(dirToDesiredVelocity);
			if (distToDesiredVelocity > 0.0001f) {
				gradient += dirToDesiredVelocity * (DesiredVelocityWeight/distToDesiredVelocity);
				value += distToDesiredVelocity * DesiredVelocityWeight;
			}

			// Prefer speeds lower or equal to the desired speed
			// and avoid speeds greater than the max speed
			var sqrSpeed = math.lengthsq(p);
			if (sqrSpeed > desiredSpeed*desiredSpeed) {
				var speed = math.sqrt(sqrSpeed);

				if (speed > maxSpeed) {
					const float MaxSpeedWeight = 3;
					value += MaxSpeedWeight * (speed - maxSpeed);
					gradient -= MaxSpeedWeight * (p/speed);
				}

				// Scale needs to be strictly greater than DesiredVelocityWeight
				// otherwise the agent will not prefer the desired speed over
				// the maximum speed
				float scale = 2*DesiredVelocityWeight;
				value += scale * (speed - desiredSpeed);
				gradient -= scale * (p/speed);
			}

			return gradient;
		}
	}

	/// <summary>
	/// Velocity Obstacle.
	/// This is a struct to avoid too many allocations.
	///
	/// See: https://en.wikipedia.org/wiki/Velocity_obstacle
	/// </summary>
	internal struct VO {
		float2 line1, line2, dir1, dir2;

		float2 cutoffLine, cutoffDir;
		float2 circleCenter;

		bool colliding;
		float radius;
		float weightFactor;
		float weightBonus;

		static float Sqr (float x) {
			return x * x;
		}

		//float2 segmentStart, segmentEnd;
		bool segment;

		public void Draw (CommandBuilder draw, float2 circleCenter, float2 drawingOffset) {
			//draw.Line(FromXZ(line1 + drawingOffset), FromXZ(line1 + dir1 + drawingOffset), Color.blue);
			//draw.Line(FromXZ(line2 + drawingOffset), FromXZ(line2 + dir2 + drawingOffset), Color.blue);
		}

		/// <summary>Creates a VO for avoiding another agent.</summary>
		/// <param name="center">The position of the other agent relative to this agent.</param>
		/// <param name="offset">Offset of the velocity obstacle. For example to account for the agents' relative velocities.</param>
		/// <param name="radius">Combined radius of the two agents (radius1 + radius2).</param>
		/// <param name="inverseDt">1 divided by the local avoidance time horizon (e.g avoid agents that we will hit within the next 2 seconds).</param>
		/// <param name="inverseDeltaTime">1 divided by the time step length.</param>
		public VO (float2 center, float2 offset, float radius, float inverseDt, float inverseDeltaTime) {
			// Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
			this.weightFactor = 1;
			weightBonus = 0;

			//this.radius = radius;
			float2 globalCenter;

			circleCenter = center*inverseDt + offset;

			this.weightFactor = 4*math.exp(-Sqr(math.lengthsq(center)/(radius*radius))) + 1;
			// Collision?
			if (math.length(center) < radius) {
				colliding = true;

				// 0.001 is there to make sure lin1.magnitude is not so small that the normalization
				// below will return Vector2.zero as that will make the VO invalid and it will be ignored.
				line1 = math.normalizesafe(center) * (math.length(center) - radius - 0.001f) * 0.3f * inverseDeltaTime;
				dir1 = math.normalize(new float2(line1.y, -line1.x));
				line1 += offset;

				cutoffDir = float2.zero;
				cutoffLine = float2.zero;
				dir2 = float2.zero;
				line2 = float2.zero;
				this.radius = 0;
			} else {
				colliding = false;

				center *= inverseDt;
				radius *= inverseDt;
				globalCenter = center+offset;

				// 0.001 is there to make sure cutoffDistance is not so small that the normalization
				// below will return Vector2.zero as that will make the VO invalid and it will be ignored.
				var cutoffDistance = math.length(center) - radius + 0.001f;

				cutoffLine = math.normalize(center) * cutoffDistance;
				// TOOD: Can be optimized!!
				cutoffDir = math.normalize(new Vector2(-cutoffLine.y, cutoffLine.x));
				cutoffLine += offset;

				float alpha = math.atan2(-center.y, -center.x);

				// Note: length has been changed here, so it needs to be recalculated
				float delta = math.abs(math.acos(radius/math.length(center)));

				this.radius = radius;

				// Bounding Lines

				// Point on circle
				line1 = new float2(math.cos(alpha+delta), math.sin(alpha+delta));
				// Vector tangent to circle which is the correct line tangent
				// Note that this vector is normalized
				dir1 = new float2(line1.y, -line1.x);

				// Point on circle
				line2 = new float2(math.cos(alpha-delta), math.sin(alpha-delta));
				// Vector tangent to circle which is the correct line tangent
				// Note that this vector is normalized
				dir2 = new float2(line2.y, -line2.x);

				line1 = line1 * radius + globalCenter;
				line2 = line2 * radius + globalCenter;
			}

			//segmentStart = float2.zero;
			//segmentEnd = float2.zero;
			segment = false;
		}

		/// <summary>
		/// Returns a negative number of if p lies on the left side of a line which with one point in a and has a tangent in the direction of dir.
		/// The number can be seen as the double signed area of the triangle {a, a+dir, p} multiplied by the length of dir.
		/// If dir.magnitude=1 this is also the distance from p to the line {a, a+dir}.
		/// </summary>
		public static float SignedDistanceFromLine (float2 a, float2 dir, float2 p) {
			return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
		}

		/// <summary>
		/// Gradient and value of the cost function of this VO.
		/// Very similar to the <see cref="Gradient"/> method however the gradient
		/// and value have been scaled and tweaked slightly.
		/// </summary>
		public float2 ScaledGradient (float2 p, out float weight) {
			var grad = Gradient(p, out weight);

			if (weight > 0) {
				const float Scale = 2;
				grad *= Scale * weightFactor;
				weight *= Scale * weightFactor;
				weight += 1 + weightBonus;
			}

			return grad;
		}

		static float2 Normalize (float2 v, out float length) {
			length = math.length(v);
			return v / length;
		}

		/// <summary>
		/// Gradient and value of the cost function of this VO.
		/// The VO has a cost function which is 0 outside the VO
		/// and increases inside it as the point moves further into
		/// the VO.
		///
		/// This is the negative gradient of that function as well as its
		/// value (the weight). The negative gradient points in the direction
		/// where the function decreases the fastest.
		///
		/// The value of the function is the distance to the closest edge
		/// of the VO and the gradient is normalized.
		/// </summary>
		public float2 Gradient (float2 p, out float weight) {
			if (colliding) {
				// Calculate double signed area of the triangle consisting of the points
				// {line1, line1+dir1, p}
				float l1 = SignedDistanceFromLine(line1, dir1, p);

				// Serves as a check for which side of the line the point p is
				if (l1 >= 0) {
					weight = l1;
					return new float2(-dir1.y, dir1.x);
				} else {
					weight = 0;
					return float2.zero;
				}
			}

			float det3 = SignedDistanceFromLine(cutoffLine, cutoffDir, p);
			if (det3 <= 0) {
				weight = 0;
				return float2.zero;
			} else {
				// Signed distances to the two edges along the sides of the VO
				float det1 = SignedDistanceFromLine(line1, dir1, p);
				float det2 = SignedDistanceFromLine(line2, dir2, p);
				if (det1 >= 0 && det2 >= 0) {
					// We are inside both of the half planes
					// (all three if we count the cutoff line)
					// and thus inside the forbidden region in velocity space

					// Actually the negative gradient because we want the
					// direction where it slopes the most downwards, not upwards
					float2 gradient;

					// Check if we are in the semicircle region near the cap of the VO
					if (math.dot(p - line1, dir1) > 0 && math.dot(p - line2, dir2) < 0) {
						if (segment) {
						} else {
							var dirFromCenter = p - circleCenter;
							float distToCenter;
							gradient = Normalize(dirFromCenter, out distToCenter);
							// The weight is the distance to the edge
							weight = radius - distToCenter;
							return gradient;
						}
					}

					if (segment && det3 < det1 && det3 < det2) {
						weight = det3;
						gradient = new float2(-cutoffDir.y, cutoffDir.x);
						return gradient;
					}

					// Just move towards the closest edge
					// The weight is the distance to the edge
					if (det1 < det2) {
						weight = det1;
						gradient = new float2(-dir1.y, dir1.x);
					} else {
						weight = det2;
						gradient = new float2(-dir2.y, dir2.x);
					}

					return gradient;
				}

				weight = 0;
				return float2.zero;
			}
		}
	}

	struct HalfPlane {
		public float2 normal;
		public float offset;

		public HalfPlane (float2 point, float2 normal) {
			this.normal = normal;
			offset = -math.dot(point, normal);
		}

		public float Distance (float2 p) {
			return math.dot(p, normal) + offset;
		}

		[BurstDiscard]
		public void Draw (CommandBuilder draw, float2 around, float2 drawingOffset) {
			// var d = Distance(around);
			// var pointOnLine = around - d * normal;
			// var tangent = new float2(normal.y, -normal.x);

			//draw.Line(FromXZ(around + drawingOffset), FromXZ(pointOnLine + drawingOffset));
			//draw.Line(FromXZ(pointOnLine - tangent * d + drawingOffset), FromXZ(pointOnLine + tangent * d + drawingOffset), Color.blue);
		}

		[BurstDiscard]
		public static void Draw (CommandBuilder draw, float2 normal, float offset, float2 around, float2 drawingOffset) {
			// var d = math.dot(around, normal) + offset;
			// var pointOnLine = around - d * normal;
			// var tangent = new float2(normal.y, -normal.x);

			//draw.Line(FromXZ(around + drawingOffset), FromXZ(pointOnLine + drawingOffset));
			//draw.Line(FromXZ(pointOnLine - tangent * d + drawingOffset), FromXZ(pointOnLine + tangent * d + drawingOffset), Color.blue);
		}
	}

	internal struct VO2 {
		float2 n1, n2, n3, n4;
		float4 halfPlaneOffsets;

		float weightFactor;
		float weightBonus;

		[BurstDiscard]
		public void Draw (CommandBuilder draw, float2 circleCenter, float2 drawingOffset) {
			HalfPlane.Draw(draw, n1, halfPlaneOffsets[0], circleCenter, drawingOffset);
			HalfPlane.Draw(draw, n2, halfPlaneOffsets[1], circleCenter, drawingOffset);
			HalfPlane.Draw(draw, n3, halfPlaneOffsets[2], circleCenter, drawingOffset);
			HalfPlane.Draw(draw, n4, halfPlaneOffsets[3], circleCenter, drawingOffset);
		}

		static float Sqr (float x) {
			return x * x;
		}

		/// <summary>Creates a VO for avoiding another agent.</summary>
		/// <param name="center">The position of the other agent relative to this agent.</param>
		/// <param name="offset">Offset of the velocity obstacle. For example to account for the agents' relative velocities.</param>
		/// <param name="radius">Combined radius of the two agents (radius1 + radius2).</param>
		/// <param name="inverseDt">1 divided by the local avoidance time horizon (e.g avoid agents that we will hit within the next 2 seconds).</param>
		/// <param name="inverseDeltaTime">1 divided by the time step length.</param>
		public VO2 (float2 center, float2 offset, float radius, float inverseDt, float inverseDeltaTime) {
			// Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
			this.weightFactor = 1;
			weightBonus = 0;

			var circleCenter = center*inverseDt + offset;

			this.weightFactor = 4*math.exp(-Sqr(math.lengthsq(center)/(radius*radius))) + 1;
			var centerMagnitude = math.length(center);

			// Collision?
			if (centerMagnitude < radius) {
				// 0.001 is there to make sure lin1.magnitude is not so small that the normalization
				// below will return Vector2.zero as that will make the VO invalid and it will be ignored.
				var normal1 = math.normalizesafe(center);
				var pos1 = normal1 * (centerMagnitude - radius - 0.001f) * 0.3f * inverseDeltaTime;
				var h1 = new HalfPlane(pos1 + offset, normal1);

				n1 = n2 = n3 = n4 = h1.normal;
				halfPlaneOffsets = new float4(h1.offset, h1.offset, h1.offset, h1.offset);
			} else {
				center *= inverseDt;
				radius *= inverseDt;

				float alpha = math.atan2(-center.y, -center.x);

				// Note: length has been changed here, so it needs to be recalculated
				float delta = math.abs(math.acos(radius/math.length(center)));

				// Bounding Lines

				// Point on circle
				var normal1 = new float2(math.cos(alpha+delta), math.sin(alpha+delta));
				var h1 = new HalfPlane(normal1 * radius + circleCenter, -normal1);

				var normal2 = new float2(math.cos(alpha-delta), math.sin(alpha-delta));
				var h2 = new HalfPlane(normal2 * radius + circleCenter, -normal2);

				var normal3 = new float2(math.cos(alpha+delta*0.5f), math.sin(alpha+delta*0.5f));
				var h3 = new HalfPlane(normal3 * radius + circleCenter, -normal3);

				var normal4 = new float2(math.cos(alpha-delta*0.5f), math.sin(alpha-delta*0.5f));
				var h4 = new HalfPlane(normal4 * radius + circleCenter, -normal4);

				n1 = h1.normal;
				n2 = h2.normal;
				n3 = h3.normal;
				n4 = h4.normal;
				halfPlaneOffsets = new float4(h1.offset, h2.offset, h3.offset, h4.offset);
			}
		}

		/// <summary>
		/// Returns a negative number of if p lies on the left side of a line which with one point in a and has a tangent in the direction of dir.
		/// The number can be seen as the double signed area of the triangle {a, a+dir, p} multiplied by the length of dir.
		/// If dir.magnitude=1 this is also the distance from p to the line {a, a+dir}.
		/// </summary>
		public static float SignedDistanceFromLine (float2 a, float2 dir, float2 p) {
			return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
		}

		/// <summary>
		/// Gradient and value of the cost function of this VO.
		/// Very similar to the <see cref="Gradient"/> method however the gradient
		/// and value have been scaled and tweaked slightly.
		/// </summary>
		public float2 ScaledGradient (float2 p, out float weight) {
			var grad = Gradient(p, out weight);

			if (weight > 0) {
				const float Scale = 2;
				grad *= Scale * weightFactor;
				weight *= Scale * weightFactor;
				weight += 1 + weightBonus;
			}

			return grad;
		}

		static float2 Normalize (float2 v, out float length) {
			length = math.length(v);
			return v / length;
		}

		/// <summary>
		/// Gradient and value of the cost function of this VO.
		/// The VO has a cost function which is 0 outside the VO
		/// and increases inside it as the point moves further into
		/// the VO.
		///
		/// This is the negative gradient of that function as well as its
		/// value (the weight). The negative gradient points in the direction
		/// where the function decreases the fastest.
		///
		/// The value of the function is the distance to the closest edge
		/// of the VO and the gradient is normalized.
		/// </summary>
		public float2 Gradient (float2 p, out float weight) {
			float2 d1 = new float2(math.dot(n1, p), math.dot(n2, p)) + halfPlaneOffsets.xy;

			if (math.any(d1 < 0)) {
				weight = 0;
				return float2.zero;
			}

			float2 d2 = new float2(math.dot(n3, p), math.dot(n4, p)) + halfPlaneOffsets.zw;
			weight = math.min(math.cmin(d1), math.cmin(d2));

			if (weight < 0) {
				weight = 0;
				return float2.zero;
			}

			if (d1[0] == weight) {
				return -n1;
			}
			if (d1[1] == weight) {
				return -n2;
			}
			if (d2[0] == weight) {
				return -n3;
			}
			return -n4;
		}
	}
}
