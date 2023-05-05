using UnityEngine;
using System.Collections.Generic;
using PathfindingCore.RVO.Sampled;

using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

/// <summary>Local avoidance related classes</summary>
namespace PathfindingCore.RVO {
	using System;
	using PathfindingCore.Jobs;
	using PathfindingCore.Drawing;
	using PathfindingCore.Util;

	public interface ISimulator {
		IAgent AddAgent(IAgent agent);
		IAgent AddAgent(Vector2 position, float elevationCoordinate);
		void RemoveAgent(IAgent agent);
		ObstacleVertex AddObstacle(ObstacleVertex v);
		ObstacleVertex AddObstacle(Vector3[] vertices, float height, bool cycle = true);
		ObstacleVertex AddObstacle(Vector3[] vertices, float height, Matrix4x4 matrix, RVOLayer layer = RVOLayer.DefaultObstacle, bool cycle = true);
		ObstacleVertex AddObstacle(Vector3 a, Vector3 b, float height);
		void UpdateObstacle(ObstacleVertex obstacle, Vector3[] vertices, Matrix4x4 matrix);
		void RemoveObstacle(ObstacleVertex v);
		IReadOnlyList<IAgent> GetAgents();
		void Update();
		void ClearAgents();
		float DesiredDeltaTime { get; set; }
		float SymmetryBreakingBias { get; set; }
		bool DoubleBuffering { get; set; }
		bool HardCollisions { get; set; }
		MovementPlane MovementPlane { get; }
		bool Multithreading { get; }
	}

	public interface IMovementPlaneWrapper {
		float2 ToPlane(float3 p);
		float2 ToPlane(float3 p, out float elevation);
		float3 ToWorld(float2 p, float elevation = 0);
		void Set(SimpleMovementPlane plane);
	}

	public struct XYMovementPlane : IMovementPlaneWrapper {
		public float2 ToPlane(float3 p) => p.xy;
		public float2 ToPlane (float3 p, out float elevation) {
			elevation = p.z;
			return p.xy;
		}
		public float3 ToWorld(float2 p, float elevation = 0) => new float3(p.x, p.y, elevation);
		public void Set (SimpleMovementPlane plane) { }
	}

	public struct XZMovementPlane : IMovementPlaneWrapper {
		public float2 ToPlane(float3 p) => p.xz;
		public float2 ToPlane (float3 p, out float elevation) {
			elevation = p.y;
			return p.xz;
		}
		public float3 ToWorld(float2 p, float elevation = 0) => new float3(p.x, elevation, p.y);
		public void Set (SimpleMovementPlane plane) { }
	}

	public struct ArbitraryMovementPlane : IMovementPlaneWrapper {
		SimpleMovementPlane plane;

		public float2 ToPlane(float3 p) => plane.ToPlane(p);
		public float2 ToPlane(float3 p, out float elevation) => plane.ToPlane(p, out elevation);
		public float3 ToWorld(float2 p, float elevation = 0) => plane.ToWorld(p, elevation);
		public void Set (SimpleMovementPlane plane) {
			this.plane = plane;
		}
	}

	public struct IReadOnlySlice<T> : System.Collections.Generic.IReadOnlyList<T> {
		public T[] data;
		public int length;

		public T this[int index] => data[index];

		public int Count => length;

		public IEnumerator<T> GetEnumerator () {
			throw new System.NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
			throw new System.NotImplementedException();
		}
	}

	/// <summary>
	/// Exposes properties of an Agent class.
	///
	/// See: RVOController
	/// See: RVOSimulator
	/// </summary>
	public interface IAgent {
		/// <summary>
		/// Internal index of the agent.
		/// See: <see cref="PathfindingCore.RVO.SimulatorBurst.simulationData"/>
		/// </summary>
		int AgentIndex { get; }

		/// <summary>
		/// Position of the agent.
		/// The agent does not move by itself, a movement script has to be responsible for
		/// reading the CalculatedTargetPoint and CalculatedSpeed properties and move towards that point with that speed.
		/// This property should ideally be set every frame.
		/// </summary>
		Vector3 Position { get; set; }

		/// <summary>
		/// Optimal point to move towards to avoid collisions.
		/// The movement script should move towards this point with a speed of <see cref="CalculatedSpeed"/>.
		///
		/// See: RVOController.CalculateMovementDelta.
		/// </summary>
		Vector3 CalculatedTargetPoint { get; }

		/// <summary>
		/// Optimal speed of the agent to avoid collisions.
		/// The movement script should move towards <see cref="CalculatedTargetPoint"/> with this speed.
		/// </summary>
		float CalculatedSpeed { get; }

		/// <summary>
		/// Point towards which the agent should move.
		/// Usually you set this once per frame. The agent will try move as close to the target point as possible.
		/// Will take effect at the next simulation step.
		///
		/// Note: The system assumes that the agent will stop when it reaches the target point
		/// so if you just want to move the agent in a particular direction, make sure that you set the target point
		/// a good distance in front of the character as otherwise the system may not avoid colisions that well.
		/// What would happen is that the system (in simplified terms) would think that the agents would stop
		/// before the collision and thus it wouldn't slow down or change course. See the image below.
		/// In the image the desiredSpeed is the length of the blue arrow and the target point
		/// is the point where the black arrows point to.
		/// In the upper case the agent does not avoid the red agent (you can assume that the red
		/// agent has a very small velocity for simplicity) while in the lower case it does.\n
		/// If you are following a path a good way to pick the target point is to set it to
		/// <code>
		/// targetPoint = directionToNextWaypoint.normalized * remainingPathDistance
		/// </code>
		/// Where remainingPathDistance is the distance until the character would reach the end of the path.
		/// This works well because at the end of the path the direction to the next waypoint will just be the
		/// direction to the last point on the path and remainingPathDistance will be the distance to the last point
		/// in the path, so targetPoint will be set to simply the last point in the path. However when remainingPathDistance
		/// is large the target point will be so far away that the agent will essentially be told to move in a particular
		/// direction, which is precisely what we want.
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="targetPoint">Target point in world space.</param>
		/// <param name="desiredSpeed">Desired speed of the agent. In world units per second. The agent will try to move with this
		///      speed if possible.</param>
		/// <param name="maxSpeed">Max speed of the agent. In world units per second. If necessary (for example if another agent
		///      is on a collision trajectory towards this agent) the agent can move at this speed.
		///      Should be at least as high as desiredSpeed, but it is recommended to use a slightly
		///      higher value than desiredSpeed (for example desiredSpeed*1.2).</param>
		void SetTarget(Vector3 targetPoint, float desiredSpeed, float maxSpeed);

		/// <summary>
		/// Plane in which the agent moves.
		/// Local avoidance calculations are always done in 2D and this plane determines how to convert from 3D to 2D.
		///
		/// In a typical 3D game the agents move in the XZ plane and in a 2D game they move in the XY plane.
		/// By default this is set to the XZ plane.
		///
		/// See: <see cref="PathfindingCore.Util.GraphTransform.xyPlane"/>
		/// See: <see cref="PathfindingCore.Util.GraphTransform.xzPlane"/>
		/// </summary>
		Util.SimpleMovementPlane MovementPlane { get; set; }

		/// <summary>Locked agents will be assumed not to move</summary>
		bool Locked { get; set; }

		/// <summary>
		/// Radius of the agent in world units.
		/// Agents are modelled as circles/cylinders.
		/// </summary>
		float Radius { get; set; }

		/// <summary>
		/// Height of the agent in world units.
		/// Agents are modelled as circles/cylinders.
		/// </summary>
		float Height { get; set; }

		/// <summary>
		/// Max number of estimated seconds to look into the future for collisions with agents.
		/// As it turns out, this variable is also very good for controling agent avoidance priorities.
		/// Agents with lower values will avoid other agents less and thus you can make 'high priority agents' by
		/// giving them a lower value.
		/// </summary>
		float AgentTimeHorizon { get; set; }

		/// <summary>Max number of estimated seconds to look into the future for collisions with obstacles</summary>
		float ObstacleTimeHorizon { get; set; }

		/// <summary>
		/// Max number of agents to take into account.
		/// Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation.
		/// </summary>
		int MaxNeighbours { get; set; }

		/// <summary>Number of neighbours that the agent took into account during the last simulation step</summary>
		int NeighbourCount { get; }

		/// <summary>
		/// Specifies the avoidance layer for this agent.
		/// The <see cref="CollidesWith"/> mask on other agents will determine if they will avoid this agent.
		/// </summary>
		RVOLayer Layer { get; set; }

		/// <summary>
		/// Layer mask specifying which layers this agent will avoid.
		/// You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		///
		/// See: http://en.wikipedia.org/wiki/Mask_(computing)
		/// </summary>
		RVOLayer CollidesWith { get; set; }

		/// <summary>
		/// Determines how strongly this agent just follows the flow instead of making other agents avoid it.
		/// The default value is 0, if it is greater than zero (up to the maximum value of 1) other agents will
		/// not avoid this character as much. However it works in a different way to <see cref="Priority"/>.
		///
		/// A group of agents with FlowFollowingStrength set to a high value that all try to reach the same point
		/// will end up just settling to stationary positions around that point, none will push the others away to any significant extent.
		/// This is tricky to achieve with priorities as priorities are all relative, so setting all agents to a low priority is the same thing
		/// as not changing priorities at all.
		///
		/// Should be a value in the range [0, 1].
		///
		/// TODO: Add video
		/// </summary>
		float FlowFollowingStrength { get; set; }

		/// <summary>
		/// Draw debug information.
		///
		/// Note: Will always draw debug info in the XZ plane even if <see cref="PathfindingCore.RVO.Simulator.movementPlane"/> is set to XY.
		/// Note: Ignored if multithreading on the simulator component has been enabled
		/// since Unity's Debug API can only be called from the main thread.
		/// </summary>
		bool DebugDraw { get; set; }

		/// <summary>
		/// List of obstacle segments which were close to the agent during the last simulation step.
		/// Can be used to apply additional wall avoidance forces for example.
		/// Segments are formed by the obstacle vertex and its .next property.
		///
		/// \bug Always returns null
		/// </summary>
		[System.Obsolete()]
		List<ObstacleVertex> NeighbourObstacles { get; }

		/// <summary>
		/// How strongly other agents will avoid this agent.
		/// Usually a value between 0 and 1.
		/// Agents with similar priorities will avoid each other with an equal strength.
		/// If an agent sees another agent with a higher priority than itself it will avoid that agent more strongly.
		/// In the extreme case (e.g this agent has a priority of 0 and the other agent has a priority of 1) it will treat the other agent as being a moving obstacle.
		/// Similarly if an agent sees another agent with a lower priority than itself it will avoid that agent less.
		///
		/// In general the avoidance strength for this agent is:
		/// <code>
		/// if this.priority > 0 or other.priority > 0:
		///     avoidanceStrength = other.priority / (this.priority + other.priority);
		/// else:
		///     avoidanceStrength = 0.5
		/// </code>
		/// </summary>
		float Priority { get; set; }

		/// <summary>
		/// Callback which will be called right before avoidance calculations are started.
		/// Used to update the other properties with the most up to date values
		/// </summary>
		System.Action PreCalculationCallback { set; }

		/// <summary>
		/// Set the normal of a wall (or something else) the agent is currently colliding with.
		/// This is used to make the RVO system aware of things like physics or an agent being clamped to the navmesh.
		/// The velocity of this agent that other agents observe will be modified so that there is no component
		/// into the wall. The agent will however not start to avoid the wall, for that you will need to add RVO obstacles.
		///
		/// This value will be cleared after the next simulation step, normally it should be set every frame
		/// when the collision is still happening.
		/// </summary>
		void SetCollisionNormal(Vector3 normal);

		/// <summary>
		/// Set the current velocity of the agent.
		/// This will override the local avoidance input completely.
		/// It is useful if you have a player controlled character and want other agents to avoid it.
		///
		/// Calling this method will mark the agent as being externally controlled for 1 simulation step.
		/// Local avoidance calculations will be skipped for the next simulation step but will be resumed
		/// after that unless this method is called again.
		/// </summary>
		void ForceSetVelocity(Vector3 velocity);
	}

	/// <summary>Plane which movement is primarily happening in</summary>
	public enum MovementPlane {
		/// <summary>Movement happens primarily in the XZ plane (3D)</summary>
		XZ,
		/// <summary>Movement happens primarily in the XY plane (2D)</summary>
		XY,
		/// <summary>For curved worlds. See: spherical (view in online documentation for working links)</summary>
		Arbitrary,
	}

	// Note: RVOLayer must not be marked with the [System.Flags] attribute because then Unity will show all RVOLayer fields as mask fields
	// which we do not want
	public enum RVOLayer {
		DefaultAgent = 1 << 0,
		DefaultObstacle = 1 << 1,
		Layer2 = 1 << 2,
		Layer3 = 1 << 3,
		Layer4 = 1 << 4,
		Layer5 = 1 << 5,
		Layer6 = 1 << 6,
		Layer7 = 1 << 7,
		Layer8 = 1 << 8,
		Layer9 = 1 << 9,
		Layer10 = 1 << 10,
		Layer11 = 1 << 11,
		Layer12 = 1 << 12,
		Layer13 = 1 << 13,
		Layer14 = 1 << 14,
		Layer15 = 1 << 15,
		Layer16 = 1 << 16,
		Layer17 = 1 << 17,
		Layer18 = 1 << 18,
		Layer19 = 1 << 19,
		Layer20 = 1 << 20,
		Layer21 = 1 << 21,
		Layer22 = 1 << 22,
		Layer23 = 1 << 23,
		Layer24 = 1 << 24,
		Layer25 = 1 << 25,
		Layer26 = 1 << 26,
		Layer27 = 1 << 27,
		Layer28 = 1 << 28,
		Layer29 = 1 << 29,
		Layer30 = 1 << 30
	}

	/// <summary>
	/// Local Avoidance %Simulator.
	/// This class handles local avoidance simulation for a number of agents using
	/// Reciprocal Velocity Obstacles (RVO) and Optimal Reciprocal Collision Avoidance (ORCA).
	///
	/// This class will handle calculation of velocities from desired velocities supplied by a script.
	/// It is, however, not responsible for moving any objects in a Unity Scene. For that there are other scripts (see below).
	///
	/// Obstacles can be added and removed from the simulation, agents can also be added and removed at any time.
	/// See: RVOSimulator
	/// See: RVOAgent
	/// See: Pathfinding.RVO.IAgent
	///
	/// The implementation uses a sampling based algorithm with gradient descent to find the avoidance velocities.
	///
	/// You will most likely mostly use the wrapper class RVOSimulator.
	/// </summary>
	public class SimulatorBurst : ISimulator {
		/// <summary>
		/// Inverse desired simulation fps.
		/// See: DesiredDeltaTime
		/// </summary>
		private float desiredDeltaTime = 0.05f;

		/// <summary>Number of agents in this simulation</summary>
		int numAgents = 0;

		/// <summary>Obstacles in this simulation</summary>
		public List<ObstacleVertex> obstacles;

		/// <summary>
		/// Quadtree for this simulation.
		/// Used internally by the simulation to perform fast neighbour lookups for each agent.
		/// Please only read from this tree, do not rebuild it since that can interfere with the simulation.
		/// It is rebuilt when necessary.
		/// </summary>
		public RVOQuadtreeBurst quadtree;

		private float deltaTime;
		private float lastStep = -99999;

		JobHandle inProgressJob;

		/// <summary>Agents in this simulation</summary>
		Agent[] agents = new Agent[0];

		Action[] agentPreCalculationCallbacks = new Action[0];

		/// <summary>A copy of the the <see cref="simulationData"/> used when the calculations are running</summary>
		AgentData currentSimulationData;
		TemporaryAgentData temporaryAgentData;
		HorizonAgentData horizonAgentData;

		/// <summary>
		/// Internal simulation data.
		/// Can be used if you need very high performance access to the agent data.
		/// Normally you would use the SimulatorBurst.Agent class instead (implements the IAgent interface).
		/// </summary>
		public AgentData simulationData;

		/// <summary>
		/// Internal simulation data.
		/// Can be used if you need very high performance access to the agent data.
		/// Normally you would use the SimulatorBurst.Agent class instead (implements the IAgent interface).
		/// </summary>
		public AgentOutputData outputData;

		bool jobInProgress;
		AgentOutputData inProgressOutputData;

		public const int MaxNeighbourCount = 50;

		class Agent : IAgent {
			public SimulatorBurst simulator;
			public int agentIndex;

			public int AgentIndex { get => agentIndex; }
			public Vector3 Position { get => simulator.simulationData.position[agentIndex]; set => simulator.simulationData.position[agentIndex] = value; }
			public bool Locked { get => simulator.simulationData.locked[agentIndex]; set => simulator.simulationData.locked[agentIndex] = value; }
			public float Radius { get => simulator.simulationData.radius[agentIndex]; set => simulator.simulationData.radius[agentIndex] = value; }
			public float Height { get => simulator.simulationData.height[agentIndex]; set => simulator.simulationData.height[agentIndex] = value; }
			public float AgentTimeHorizon { get => simulator.simulationData.agentTimeHorizon[agentIndex]; set => simulator.simulationData.agentTimeHorizon[agentIndex] = value; }
			public float ObstacleTimeHorizon { get => simulator.simulationData.obstacleTimeHorizon[agentIndex]; set => simulator.simulationData.obstacleTimeHorizon[agentIndex] = value; }
			public int MaxNeighbours { get => simulator.simulationData.maxNeighbours[agentIndex]; set => simulator.simulationData.maxNeighbours[agentIndex] = value; }
			public RVOLayer Layer { get => simulator.simulationData.layer[agentIndex]; set => simulator.simulationData.layer[agentIndex] = value; }
			public RVOLayer CollidesWith { get => simulator.simulationData.collidesWith[agentIndex]; set => simulator.simulationData.collidesWith[agentIndex] = value; }
			public float FlowFollowingStrength { get => simulator.simulationData.flowFollowingStrength[agentIndex]; set => simulator.simulationData.flowFollowingStrength[agentIndex] = value; }
			public bool DebugDraw { get => simulator.simulationData.debugDraw[agentIndex]; set => simulator.simulationData.debugDraw[agentIndex] = value; }
			public float Priority { get => simulator.simulationData.priority[agentIndex]; set => simulator.simulationData.priority[agentIndex] = value; }
			public SimpleMovementPlane MovementPlane { get => simulator.simulationData.movementPlane[agentIndex]; set => simulator.simulationData.movementPlane[agentIndex] = value; }
			public Action PreCalculationCallback { set => simulator.agentPreCalculationCallbacks[agentIndex] = value; }

			public Vector3 CalculatedTargetPoint => simulator.outputData.targetPoint[agentIndex];
			public float CalculatedSpeed => simulator.outputData.speed[agentIndex];
			public int NeighbourCount => simulator.outputData.numNeighbours[agentIndex];

			public List<ObstacleVertex> NeighbourObstacles => throw new NotImplementedException();

			public void SetTarget (Vector3 targetPoint, float desiredSpeed, float maxSpeed) {
				simulator.simulationData.SetTarget(agentIndex, targetPoint, desiredSpeed, maxSpeed);
			}

			public void SetCollisionNormal (Vector3 normal) {
				simulator.simulationData.collisionNormal[agentIndex] = normal;
			}

			public void ForceSetVelocity (Vector3 velocity) {
				// A bit hacky, but it is approximately correct
				// assuming the agent does not move significantly
				// TODO: non-burst also sets simulator.outputData.targetPoint
				simulator.simulationData.targetPoint[agentIndex] = simulator.simulationData.position[agentIndex] + (float3)velocity * 1000;
				simulator.simulationData.desiredSpeed[agentIndex] = velocity.magnitude;
				// TODO: non-burst also sets CalculatedSpeed
				simulator.simulationData.manuallyControlled[agentIndex] = true;
			}
		}

		public struct AgentData {
			// Note: All 2D vectors are relative to the movement plane, all 3D vectors are in world space
			public NativeArray<float> radius;
			public NativeArray<float> height;
			public NativeArray<float> desiredSpeed;
			public NativeArray<float> maxSpeed;
			public NativeArray<float> agentTimeHorizon;
			public NativeArray<float> obstacleTimeHorizon;
			public NativeArray<bool> locked;
			public NativeArray<int> maxNeighbours;
			public NativeArray<RVOLayer> layer;
			public NativeArray<RVOLayer> collidesWith;
			public NativeArray<float> flowFollowingStrength;
			public NativeArray<float3> position;
			public NativeArray<float3> collisionNormal;
			public NativeArray<bool> manuallyControlled;
			public NativeArray<float> priority;
			public NativeArray<bool> debugDraw;
			public NativeArray<float3> targetPoint;
			public NativeArray<SimpleMovementPlane> movementPlane;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref radius, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref height, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref desiredSpeed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref maxSpeed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref agentTimeHorizon, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref obstacleTimeHorizon, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref locked, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref maxNeighbours, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref layer, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collidesWith, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref flowFollowingStrength, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref position, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collisionNormal, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref manuallyControlled, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref priority, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref debugDraw, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref targetPoint, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref movementPlane, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void SetTarget (int agentIndex, float3 targetPoint, float desiredSpeed, float maxSpeed) {
				maxSpeed = math.max(maxSpeed, 0);
				desiredSpeed = math.clamp(desiredSpeed, 0, maxSpeed);

				this.targetPoint[agentIndex] = targetPoint;
				this.desiredSpeed[agentIndex] = desiredSpeed;
				this.maxSpeed[agentIndex] = maxSpeed;
			}

			public void Move (int fromIndex, int toIndex) {
				radius[toIndex] = radius[fromIndex];
				height[toIndex] = height[fromIndex];
				desiredSpeed[toIndex] = desiredSpeed[fromIndex];
				maxSpeed[toIndex] = maxSpeed[fromIndex];
				agentTimeHorizon[toIndex] = agentTimeHorizon[fromIndex];
				obstacleTimeHorizon[toIndex] = obstacleTimeHorizon[fromIndex];
				locked[toIndex] = locked[fromIndex];
				maxNeighbours[toIndex] = maxNeighbours[fromIndex];
				layer[toIndex] = layer[fromIndex];
				collidesWith[toIndex] = collidesWith[fromIndex];
				flowFollowingStrength[toIndex] = flowFollowingStrength[fromIndex];
				position[toIndex] = position[fromIndex];
				collisionNormal[toIndex] = collisionNormal[fromIndex];
				manuallyControlled[toIndex] = manuallyControlled[fromIndex];
				priority[toIndex] = priority[fromIndex];
				debugDraw[toIndex] = debugDraw[fromIndex];
				targetPoint[toIndex] = targetPoint[fromIndex];
				movementPlane[toIndex] = movementPlane[fromIndex];
			}

			public void Dispose () {
				radius.Dispose();
				height.Dispose();
				desiredSpeed.Dispose();
				maxSpeed.Dispose();
				agentTimeHorizon.Dispose();
				obstacleTimeHorizon.Dispose();
				locked.Dispose();
				maxNeighbours.Dispose();
				layer.Dispose();
				collidesWith.Dispose();
				flowFollowingStrength.Dispose();
				position.Dispose();
				collisionNormal.Dispose();
				manuallyControlled.Dispose();
				priority.Dispose();
				debugDraw.Dispose();
				targetPoint.Dispose();
				movementPlane.Dispose();
			}
		};

		public struct AgentOutputData {
			public NativeArray<float3> targetPoint;
			public NativeArray<float> speed;
			public NativeArray<int> numNeighbours;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref targetPoint, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref speed, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref numNeighbours, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Move (int fromIndex, int toIndex) {
				targetPoint[toIndex] = targetPoint[fromIndex];
				speed[toIndex] = speed[fromIndex];
				numNeighbours[toIndex] = numNeighbours[fromIndex];
			}

			public void Dispose () {
				targetPoint.Dispose();
				speed.Dispose();
				numNeighbours.Dispose();
			}
		};

		public struct HorizonAgentData {
			public NativeArray<int> horizonSide;
			public NativeArray<float> horizonMinAngle;
			public NativeArray<float> horizonMaxAngle;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref horizonSide, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref horizonMinAngle, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref horizonMaxAngle, size, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Move (int fromIndex, int toIndex) {
				horizonSide[toIndex] = horizonSide[fromIndex];
				// The other values are temporary values that don't have to be moved
			}

			public void Dispose () {
				horizonSide.Dispose();
				horizonMinAngle.Dispose();
				horizonMaxAngle.Dispose();
			}
		}

		public struct TemporaryAgentData {
			public NativeArray<float2> desiredTargetPointInVelocitySpace;
			public NativeArray<float3> desiredVelocity;
			public NativeArray<float3> currentVelocity;
			public NativeArray<float2> collisionVelocityOffsets;
			public NativeArray<int> neighbours;

			public void Realloc (int size, Allocator allocator) {
				Util.Memory.Realloc(ref desiredTargetPointInVelocitySpace, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref desiredVelocity, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref currentVelocity, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref collisionVelocityOffsets, size, allocator, NativeArrayOptions.UninitializedMemory);
				Util.Memory.Realloc(ref neighbours, size * MaxNeighbourCount, allocator, NativeArrayOptions.UninitializedMemory);
			}

			public void Dispose () {
				desiredTargetPointInVelocitySpace.Dispose();
				desiredVelocity.Dispose();
				currentVelocity.Dispose();
				neighbours.Dispose();
				collisionVelocityOffsets.Dispose();
			}
		}


		public float DeltaTime => deltaTime;

		/// <summary>
		/// Time in seconds between each simulation step.
		/// This is the desired delta time, the simulation will never run at a higher fps than
		/// the rate at which the Update function is called.
		/// </summary>
		public float DesiredDeltaTime { get { return desiredDeltaTime; } set { desiredDeltaTime = System.Math.Max(value, 0.0f); } }

		/// <summary>
		/// Bias agents to pass each other on the right side.
		/// If the desired velocity of an agent puts it on a collision course with another agent or an obstacle
		/// its desired velocity will be rotated this number of radians (1 radian is approximately 57°) to the right.
		/// This helps to break up symmetries and makes it possible to resolve some situations much faster.
		///
		/// When many agents have the same goal this can however have the side effect that the group
		/// clustered around the target point may as a whole start to spin around the target point.
		///
		/// Recommended values are in the range of 0 to 0.2.
		///
		/// If this value is negative, the agents will be biased towards passing each other on the left side instead.
		/// </summary>
		public float SymmetryBreakingBias { get; set; }

		/// <summary>Use double buffering</summary>
		public bool DoubleBuffering { get; set; }

		/// <summary>Use hard collisions</summary>
		public bool HardCollisions { get; set; }

		public Rect AgentBounds => quadtree.bounds;

		/// <summary>Number of agents in the simulation</summary>
		public int AgentCount => numAgents;

		public MovementPlane MovementPlane => movementPlane;

		public bool Multithreading => true;

		/// <summary>True if at least one agent had debug drawing enabled during the previous frame</summary>
		public bool AnyAgentHasDebug { get; private set; }

		/// <summary>Determines if the XY (2D) or XZ (3D) plane is used for movement</summary>
		public readonly MovementPlane movementPlane = MovementPlane.XZ;

		/// <summary>
		/// Get a list of all agents.
		///
		/// This is an internal list.
		/// I'm not going to be restrictive so you may access it since it is better for performance
		/// but please do not modify it since that can cause errors in the simulation.
		///
		/// Warning: Do not modify this list!
		/// </summary>
		public IReadOnlyList<IAgent> GetAgents () {
			return new IReadOnlySlice<IAgent> {
					   data = agents,
					   length = numAgents,
			};
		}

		/// <summary>
		/// Get a list of all obstacles.
		/// This is a list of obstacle vertices.
		/// Each vertex is part of a doubly linked list loop
		/// forming an obstacle polygon.
		///
		/// Warning: Do not modify this list!
		///
		/// See: AddObstacle
		/// See: RemoveObstacle
		/// </summary>
		public List<ObstacleVertex> GetObstacles () {
			return obstacles;
		}

		/// <summary>
		/// Create a new simulator.
		///
		/// Note: Will only have effect if using multithreading
		///
		/// See: <see cref="Multithreading"/>
		/// </summary>
		/// <param name="workers">Use the specified number of worker threads.\n
		/// When the number zero is specified, no multithreading will be used.
		/// A good number is the number of cores on the machine.</param>
		/// <param name="doubleBuffering">Use Double Buffering for calculations.
		/// Testing done with 5000 agents and 0.1 desired delta time showed that with double buffering enabled
		/// the game ran at 50 fps for most frames, dropping to 10 fps during calculation frames. But without double buffering
		/// it ran at around 10 fps all the time.\n
		/// This will let threads calculate while the game progresses instead of waiting for the calculations
		/// to finish.</param>
		/// <param name="movementPlane">The plane that the movement happens in. XZ for 3D games, XY for 2D games.</param>
		public SimulatorBurst (bool doubleBuffering, MovementPlane movementPlane) {
			this.DoubleBuffering = doubleBuffering;
			this.DesiredDeltaTime = 1;
			this.movementPlane = movementPlane;

			obstacles = new List<ObstacleVertex>();
			AllocateAgentSpace();

			// Just to make sure the quadtree is in a valid state
			quadtree.BuildJob(simulationData.position, simulationData.desiredSpeed, simulationData.radius, 0, movementPlane);
		}

		/// <summary>Removes all agents from the simulation</summary>
		public void ClearAgents () {
			// Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			for (int i = 0; i < agents.Length; i++) {
				if (agents[i] != null) {
					agents[i].simulator = null;
					agents[i].agentIndex = -1;
					agents[i] = null;
				}
			}
			numAgents = 0;
		}

		/// <summary>
		/// Frees all used memory.
		/// Warning: You must call this when you are done with the simulator, otherwise some resources can linger and lead to memory leaks.
		/// </summary>
		public void OnDestroy () {
			ClearAgents();
			currentSimulationData.Dispose();
			simulationData.Dispose();
			temporaryAgentData.Dispose();
			inProgressOutputData.Dispose();
			outputData.Dispose();
			quadtree.Dispose();
			horizonAgentData.Dispose();
		}

		void AllocateAgentSpace () {
			if (numAgents > agentPreCalculationCallbacks.Length || agentPreCalculationCallbacks.Length == 0) {
				BlockUntilSimulationStepIsDone();
				int newSize = Mathf.Max(64, Mathf.Max(numAgents, agentPreCalculationCallbacks.Length * 2));
				currentSimulationData.Realloc(newSize, Allocator.Persistent);
				simulationData.Realloc(newSize, Allocator.Persistent);
				temporaryAgentData.Realloc(newSize, Allocator.Persistent);
				outputData.Realloc(newSize, Allocator.Persistent);
				inProgressOutputData.Realloc(newSize, Allocator.Persistent);
				horizonAgentData.Realloc(newSize, Allocator.Persistent);

				var newAgentPreCalculationCallbacks = new Action[newSize];
				agentPreCalculationCallbacks.CopyTo(newAgentPreCalculationCallbacks, 0);
				agentPreCalculationCallbacks = newAgentPreCalculationCallbacks;

				var newAgents = new Agent[newSize];
				agents.CopyTo(newAgents, 0);
				agents = newAgents;
			}
		}

		/// <summary>
		/// Add an agent at the specified position.
		/// You can use the returned interface to read and write parameters
		/// and set for example radius and desired point to move to.
		///
		/// See: <see cref="RemoveAgent"/>
		///
		/// Deprecated: Use AddAgent(Vector3) instead
		/// </summary>
		/// <param name="position">See #IAgent.Position</param>
		/// <param name="elevationCoordinate">See IAgent.ElevationCoordinate</param>
		[System.Obsolete("Use AddAgent(Vector3) instead")]
		public IAgent AddAgent (Vector2 position, float elevationCoordinate) {
			if (movementPlane == MovementPlane.XY) return AddAgent(new Vector3(position.x, position.y, elevationCoordinate));
			else return AddAgent(new Vector3(position.x, elevationCoordinate, position.y));
		}

		/// <summary>
		/// Add an agent at the specified position.
		/// You can use the returned interface to read and write parameters
		/// and set for example radius and desired point to move to.
		///
		/// See: <see cref="RemoveAgent"/>
		/// </summary>
		/// <param name="position">See #IAgent.Position</param>
		public IAgent AddAgent (Vector3 position) {
			int agentIndex = numAgents;

			numAgents++;
			AllocateAgentSpace();

			var agent = agents[agentIndex] = new Agent { simulator = this, agentIndex = agentIndex };

			simulationData.radius[agentIndex] = 5;
			simulationData.height[agentIndex] = 5;
			simulationData.desiredSpeed[agentIndex] = 0;
			simulationData.maxSpeed[agentIndex] = 1;
			simulationData.agentTimeHorizon[agentIndex] = 2;
			simulationData.obstacleTimeHorizon[agentIndex] = 2;
			simulationData.locked[agentIndex] = false;
			simulationData.maxNeighbours[agentIndex] = 10;
			simulationData.layer[agentIndex] = RVOLayer.DefaultAgent;
			simulationData.collidesWith[agentIndex] = (RVOLayer)(-1);
			simulationData.flowFollowingStrength[agentIndex] = 0;
			simulationData.position[agentIndex] = position;
			simulationData.collisionNormal[agentIndex] = float3.zero;
			simulationData.manuallyControlled[agentIndex] = false;
			simulationData.priority[agentIndex] = 0.5f;
			simulationData.debugDraw[agentIndex] = false;
			simulationData.targetPoint[agentIndex] = position;
			// Set the default movement plane. Default to the XZ plane even if movement plane is arbitrary (the user will have to set a custom one later)
			simulationData.movementPlane[agentIndex] = movementPlane == MovementPlane.XY ? SimpleMovementPlane.XYPlane : SimpleMovementPlane.XZPlane;

			outputData.speed[agentIndex] = 0;
			outputData.numNeighbours[agentIndex] = 0;
			outputData.targetPoint[agentIndex] = position;

			horizonAgentData.horizonSide[agentIndex] = 0;
			agentPreCalculationCallbacks[agentIndex] = null;

			return agent;
		}

		public IAgent AddAgent (IAgent agent) {
			throw new System.NotImplementedException("Use AddAgent(position) instead. Agents are not persistent after being removed.");
		}

		/// <summary>
		/// Removes a specified agent from this simulation.
		/// The agent can be added again later by using AddAgent.
		///
		/// See: AddAgent(IAgent)
		/// See: ClearAgents
		/// </summary>
		public void RemoveAgent (IAgent agent) {
			if (agent == null) throw new System.ArgumentNullException(nameof(agent));
			// Don't interfere with ongoing calculations
			BlockUntilSimulationStepIsDone();

			Agent realAgent = (Agent)agent;

			// Already removed
			if (realAgent.simulator == null) return;

			numAgents--;
			currentSimulationData.Move(numAgents, realAgent.agentIndex);
			simulationData.Move(numAgents, realAgent.agentIndex);
			outputData.Move(numAgents, realAgent.agentIndex);
			horizonAgentData.Move(numAgents, realAgent.agentIndex);
			agentPreCalculationCallbacks[realAgent.agentIndex] = agentPreCalculationCallbacks[numAgents];
			agents[realAgent.agentIndex] = agents[numAgents];
			agents[realAgent.agentIndex].agentIndex = realAgent.agentIndex;

			// Avoid memory leaks
			agentPreCalculationCallbacks[numAgents] = null;

			realAgent.simulator = null;
			realAgent.agentIndex = -1;
		}

		public ObstacleVertex AddObstacle (ObstacleVertex v) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		public ObstacleVertex AddObstacle (Vector3[] vertices, float height, bool cycle = true) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		public ObstacleVertex AddObstacle (Vector3[] vertices, float height, Matrix4x4 matrix, RVOLayer layer = RVOLayer.DefaultObstacle, bool cycle = true) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		public ObstacleVertex AddObstacle (Vector3 a, Vector3 b, float height) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		public void UpdateObstacle (ObstacleVertex obstacle, Vector3[] vertices, Matrix4x4 matrix) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		public void RemoveObstacle (ObstacleVertex v) {
			throw new System.NotImplementedException("Local avoidance obstacles have been removed. They never worked particularly well and the performance was poor. Use regular obstacles that update the pathfinding graphs instead.");
		}

		private void ScheduleCleanObstacles () {
		}

		private void CleanObstacles () {
		}

		/// <summary>
		/// Rebuilds the obstacle tree at next simulation frame.
		/// Add and remove obstacle functions call this automatically.
		/// </summary>
		public void UpdateObstacles () {
			// Update obstacles at next frame
			//doUpdateObstacles = true;
		}

		/// <summary>
		/// Blocks until separate threads have finished with the current simulation step.
		/// When double buffering is done, the simulation is performed in between frames.
		/// </summary>
		void BlockUntilSimulationStepIsDone () {
			if (jobInProgress) {
				inProgressJob.Complete();
				Util.Memory.Swap(ref outputData, ref inProgressOutputData);
				jobInProgress = false;
			}
		}

		void PreCalculation () {
			for (int i = 0; i < numAgents; i++) agentPreCalculationCallbacks[i]?.Invoke();
		}

		void CleanAndUpdateObstaclesIfNecessary () {
		}

		/// <summary>Should be called once per frame</summary>
		public void Update () {
			// The burst jobs are specialized for the type of movement plane used. This improves performance for the XY and XZ movement planes quite a lot
			if (movementPlane == MovementPlane.XY) UpdateInternal<XYMovementPlane>();
			else if (movementPlane == MovementPlane.XZ) UpdateInternal<XZMovementPlane>();
			else UpdateInternal<ArbitraryMovementPlane>();
		}

		void UpdateInternal<T> () where T : struct, IMovementPlaneWrapper {
			// Initialize last step
			if (lastStep < 0) {
				lastStep = Time.time;
				deltaTime = DesiredDeltaTime;
			}

			if (Time.time - lastStep >= DesiredDeltaTime) {
				BlockUntilSimulationStepIsDone();
				deltaTime = Time.time - lastStep;
				lastStep = Time.time;

				// Prevent a zero delta time
				deltaTime = math.max(deltaTime, 1.0f/2000f);

				UnityEngine.Profiling.Profiler.BeginSample("Read agent data");
				PreCalculation();
				CleanAndUpdateObstaclesIfNecessary();
				UnityEngine.Profiling.Profiler.EndSample();

				var quadtreeJob = quadtree.BuildJob(simulationData.position, outputData.speed, simulationData.radius, numAgents, movementPlane).Schedule();

				var anyDebugEnabled = new NativeArray<bool>(1, Allocator.TempJob);

				var copyJob = new JobRVOCopy {
					agentData = simulationData,
					agentDataCopy = currentSimulationData,
					anyDebugEnabled = anyDebugEnabled,
					startIndex = 0,
					endIndex = numAgents,
				}.Schedule();

				var preprocessJob = new JobRVOPreprocess {
					agentData = simulationData,
					previousOutput = outputData,
					temporaryAgentData = temporaryAgentData,
					startIndex = 0,
					endIndex = numAgents,
				}.Schedule();

				// Clear the normal
				// The normal is reset every simulation tick
				// Note that the copied data still contains the real normal.
				var clearJob = simulationData.collisionNormal.MemSet(float3.zero).Schedule(JobHandle.CombineDependencies(copyJob, preprocessJob));

				int batchSize = math.max(numAgents / 32, 4);
				var neighboursJob = new JobRVOCalculateNeighbours<T> {
					agentData = currentSimulationData,
					quadtree = quadtree,
					outNeighbours = temporaryAgentData.neighbours,
					output = outputData,
				}.ScheduleBatch(numAgents, batchSize, JobHandle.CombineDependencies(copyJob, quadtreeJob, preprocessJob));

				var combinedJob = JobHandle.CombineDependencies(preprocessJob, neighboursJob);

				var draw = DrawingManager.GetBuilder();

				var horizonJob1 = new JobHorizonAvoidancePhase1 {
					agentData = currentSimulationData,
					neighbours = temporaryAgentData.neighbours,
					desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace,
					horizonAgentData = horizonAgentData,
					draw = draw,
				}.ScheduleBatch(numAgents, batchSize, combinedJob);

				var horizonJob2 = new JobHorizonAvoidancePhase2 {
					neighbours = temporaryAgentData.neighbours,
					desiredVelocity = temporaryAgentData.desiredVelocity,
					desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace,
					horizonAgentData = horizonAgentData,
					movementPlane = currentSimulationData.movementPlane,
				}.ScheduleBatch(numAgents, batchSize, horizonJob1);

				var hardCollisionsJob1 = new JobHardCollisions<T> {
					agentData = currentSimulationData,
					neighbours = temporaryAgentData.neighbours,
					collisionVelocityOffsets = temporaryAgentData.collisionVelocityOffsets,
					deltaTime = deltaTime,
					enabled = HardCollisions,
				}.ScheduleBatch(numAgents, batchSize, combinedJob);

				var rvoJobData = new JobRVO<T> {
					agentData = currentSimulationData,
					temporaryAgentData = temporaryAgentData,
					output = DoubleBuffering ? inProgressOutputData : outputData,
					deltaTime = deltaTime,
					symmetryBreakingBias = Mathf.Max(0, SymmetryBreakingBias),
					draw = draw,
				};

				combinedJob = JobHandle.CombineDependencies(horizonJob2, hardCollisionsJob1);

				copyJob.Complete();
				AnyAgentHasDebug = anyDebugEnabled[0];

				var rvoJob = rvoJobData.ScheduleBatch(numAgents, batchSize, combinedJob);

				JobHandle.ScheduleBatchedJobs();
				JobHandle.CombineDependencies(combinedJob, clearJob).Complete();

				if (DoubleBuffering && !anyDebugEnabled[0]) {
					inProgressJob = rvoJob;
					jobInProgress = true;
				} else {
					rvoJob.Complete();
				}

				draw.Dispose();
				anyDebugEnabled.Dispose();
				//quadtree.DebugDraw();
			}
		}
	}
}
