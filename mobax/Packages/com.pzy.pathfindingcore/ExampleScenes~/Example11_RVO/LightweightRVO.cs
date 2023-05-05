using UnityEngine;
using System.Collections.Generic;
using Pathfinding.RVO;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Pathfinding.Examples {
	[RequireComponent(typeof(MeshFilter))]
	/// <summary>
	/// Lightweight RVO Circle Example.
	/// Lightweight script for simulating agents in a circle trying to reach their antipodal positions.
	/// This script, compared to using lots of RVOAgents shows the real power of the RVO simulator when
	/// little other overhead (e.g GameObjects) is present.
	///
	/// For example with this script, I can simulate 5000 agents at around 50 fps on my laptop (with desired simulation fps = 10 and interpolation, 2 threads)
	/// however when using prefabs, only instantiating the 5000 agents takes 10 seconds and it runs at around 5 fps.
	///
	/// This script will render the agents by generating a square for each agent combined into a single mesh with appropriate UV.
	///
	/// A few GUI buttons will be drawn by this script with which the user can change the number of agents.
	/// </summary>
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_lightweight_r_v_o.php")]
	public class LightweightRVO : MonoBehaviour {
		/// <summary>Number of agents created at start</summary>
		public int agentCount = 100;

		/// <summary>
		/// How large is the area where agents are placed.
		/// For e.g the circle example, it corresponds
		/// </summary>
		public float exampleScale = 100;


		public enum RVOExampleType {
			Circle,
			Line,
			Point,
			RandomStreams,
			Crossing
		}

		public RVOExampleType type = RVOExampleType.Circle;

		/// <summary>Agent radius</summary>
		public float radius = 3;

		/// <summary>Max speed for an agent</summary>
		public float maxSpeed = 2;

		/// <summary>How far in the future too look for agents</summary>
		public float agentTimeHorizon = 10;

		[HideInInspector]
		/// <summary>How far in the future too look for obstacles</summary>
		public float obstacleTimeHorizon = 10;

		/// <summary>Max number of neighbour agents to take into account</summary>
		public int maxNeighbours = 10;

		/// <summary>
		/// Offset from the agent position the actual drawn postition.
		/// Used to get rid of z-buffer issues
		/// </summary>
		public Vector3 renderingOffset = Vector3.up*0.1f;

		/// <summary>Enable the debug flag for all agents</summary>
		public bool debug = false;

		/// <summary>Mesh for rendering</summary>
		Mesh mesh;

		/// <summary>Reference to the simulator in the scene</summary>
		Pathfinding.RVO.ISimulator sim;

		/// <summary>All agents handled by this script</summary>
		List<IAgent> agents;

		/// <summary>Goals for each agent</summary>
		NativeArray<float3> goals;

		/// <summary>Color for each agent</summary>
		NativeArray<Color> agentColors;

		NativeArray<float2> interpolatedVelocities;
		NativeArray<float2> interpolatedRotations;

		public void Start () {
			mesh = new Mesh();

			RVOSimulator rvoSim = RVOSimulator.active;
			if (rvoSim == null) {
				Debug.LogError("No RVOSimulator could be found in the scene. Please add a RVOSimulator component to any GameObject");
				return;
			}
			sim = rvoSim.GetSimulator();
			GetComponent<MeshFilter>().mesh = mesh;

			CreateAgents(agentCount);
		}

		public void OnGUI () {
			if (GUILayout.Button("2")) CreateAgents(2);
			if (GUILayout.Button("10")) CreateAgents(10);
			if (GUILayout.Button("100")) CreateAgents(100);
			if (GUILayout.Button("500")) CreateAgents(500);
			if (GUILayout.Button("1000")) CreateAgents(1000);
			if (GUILayout.Button("5000")) CreateAgents(5000);
			if (GUILayout.Button("10000")) CreateAgents(10000);
			if (GUILayout.Button("20000")) CreateAgents(20000);
			if (GUILayout.Button("30000")) CreateAgents(30000);

			GUILayout.Space(5);

			if (GUILayout.Button("Random Streams")) {
				type = RVOExampleType.RandomStreams;
				CreateAgents(agents != null ? agents.Count : 100);
			}

			if (GUILayout.Button("Line")) {
				type = RVOExampleType.Line;
				CreateAgents(agents != null ? Mathf.Min(agents.Count, 100) : 10);
			}

			if (GUILayout.Button("Circle")) {
				type = RVOExampleType.Circle;
				CreateAgents(agents != null ? agents.Count : 100);
			}

			if (GUILayout.Button("Point")) {
				type = RVOExampleType.Point;
				CreateAgents(agents != null ? agents.Count : 100);
			}

			if (GUILayout.Button("Crossing")) {
				type = RVOExampleType.Crossing;
				CreateAgents(agents != null ? agents.Count : 100);
			}
		}

		void OnDestroy () {
			agentColors.Dispose();
			goals.Dispose();
			interpolatedVelocities.Dispose();
			interpolatedRotations.Dispose();
		}

		private float uniformDistance (float radius) {
			float v = UnityEngine.Random.value + UnityEngine.Random.value;

			if (v > 1) return radius * (2-v);
			else return radius * v;
		}

		/// <summary>Create a number of agents in circle and restart simulation</summary>
		public void CreateAgents (int num) {
			this.agentCount = num;

			agents = new List<IAgent>(agentCount);
			Util.Memory.Realloc(ref goals, agentCount, Allocator.Persistent);
			Util.Memory.Realloc(ref agentColors, agentCount, Allocator.Persistent);

			sim.ClearAgents();

			if (type == RVOExampleType.Circle) {
				float agentArea = agentCount * radius * radius * Mathf.PI;
				const float EmptyFraction = 0.7f;
				const float PackingDensity = 0.9f;
				float innerCircleRadius = Mathf.Sqrt(agentArea/(Mathf.PI*(1-EmptyFraction*EmptyFraction)));
				float outerCircleRadius = Mathf.Sqrt(innerCircleRadius*innerCircleRadius + agentCount*radius*radius/PackingDensity);
				//float circleRad = Mathf.Sqrt(Mathf.Sqrt(agentCount * radius * radius * 4 / Mathf.PI)) * 0.05 * exampleScale;

				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3(Mathf.Cos(i * Mathf.PI * 2.0f / agentCount), 0, Mathf.Sin(i * Mathf.PI * 2.0f / agentCount)) * math.lerp(innerCircleRadius, outerCircleRadius, UnityEngine.Random.value);
					IAgent agent = sim.AddAgent(new Vector2(pos.x, pos.z), pos.y);
					agents.Add(agent);
					goals[i] = new float3(-pos.x, 0, -pos.z);
					agentColors[i] = AstarMath.HSVToRGB(i * 360.0f / agentCount, 0.8f, 0.6f);
				}
			} else if (type == RVOExampleType.Line) {
				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3((i % 2 == 0 ? 1 : -1) * exampleScale, 0, (i / 2) * radius * 2.5f);
					IAgent agent = sim.AddAgent(new Vector2(pos.x, pos.z), pos.y);
					agents.Add(agent);
					goals[i] = new float3(-pos.x, 0, pos.z);
					agentColors[i] = i % 2 == 0 ? Color.red : Color.blue;
				}
			} else if (type == RVOExampleType.Point) {
				for (int i = 0; i < agentCount; i++) {
					Vector3 pos = new Vector3(Mathf.Cos(i * Mathf.PI * 2.0f / agentCount), 0, Mathf.Sin(i * Mathf.PI * 2.0f / agentCount)) * exampleScale;
					IAgent agent = sim.AddAgent(new Vector2(pos.x, pos.z), pos.y);
					agents.Add(agent);
					goals[i] = new float3(0, 0, 0);
					agentColors[i] = AstarMath.HSVToRGB(i * 360.0f / agentCount, 0.8f, 0.6f);
				}
			} else if (type == RVOExampleType.RandomStreams) {
				float circleRad = Mathf.Sqrt(agentCount * radius * radius * 4 / Mathf.PI) * exampleScale * 0.05f;

				for (int i = 0; i < agentCount; i++) {
					float angle = UnityEngine.Random.value * Mathf.PI * 2.0f;
					float targetAngle = UnityEngine.Random.value * Mathf.PI * 2.0f;
					Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * uniformDistance(circleRad);
					IAgent agent = sim.AddAgent(new Vector2(pos.x, pos.z), pos.y);
					agents.Add(agent);
					goals[i] = new float3(math.cos(targetAngle), 0, math.sin(targetAngle)) * uniformDistance(circleRad);
					agentColors[i] = AstarMath.HSVToRGB(targetAngle * Mathf.Rad2Deg, 0.8f, 0.6f);
				}
			} else if (type == RVOExampleType.Crossing) {
				float distanceBetweenGroups = exampleScale * radius * 0.5f;
				int directions = (int)Mathf.Sqrt(agentCount / 25f);
				directions = Mathf.Max(directions, 2);

				const int AgentsPerDistance = 10;
				for (int i = 0; i < agentCount; i++) {
					float angle = ((i % directions)/(float)directions) * Mathf.PI * 2.0f;
					var dist = distanceBetweenGroups * ((i/(directions*AgentsPerDistance) + 1) + 0.3f*UnityEngine.Random.value);
					Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dist;
					IAgent agent = sim.AddAgent(new Vector2(pos.x, pos.z), pos.y);
					agent.Priority = (i % directions) == 0 ? 1 : 0.01f;
					agents.Add(agent);
					goals[i] = math.normalizesafe(new float3(-pos.x, 0, -pos.z)) * distanceBetweenGroups * 3;
					agentColors[i] = AstarMath.HSVToRGB(angle * Mathf.Rad2Deg, 0.8f, 0.6f);
				}
			}

			Update();
		}

		[BurstCompile]
		struct JobMoveAgents : IJob {
			public NativeArray<float2> interpolatedVelocities;
			public NativeArray<float2> interpolatedRotations;
			public NativeArray<float3> agentPositions;
			[ReadOnly] public NativeArray<float3> agentTargetPoints;
			[ReadOnly] public NativeArray<float> agentMaxSpeeds;
			[ReadOnly] public NativeArray<float> agentSpeeds;
			public float deltaTime;
			public int startIndex, endIndex;
			public bool debug;

			public void Execute () {
				for (int agentIndex = startIndex; agentIndex < endIndex; agentIndex++) {
					var pos = agentPositions[agentIndex];
					var deltaPosition = agentTargetPoints[agentIndex] - pos;
					float length = math.length(deltaPosition);
					// Clamp the movement delta to a maximum length
					var maxLength = agentSpeeds[agentIndex] * deltaTime;
					if (length > maxLength && length > 0) deltaPosition *= maxLength / length;

					// Move the agent
					// This is the responsibility of this script, not the RVO system
					if (!debug) {
						agentPositions[agentIndex] = pos + deltaPosition;
					}

					interpolatedVelocities[agentIndex] += deltaPosition.xz;
					float vLength = math.length(interpolatedVelocities[agentIndex]);
					if (vLength > agentMaxSpeeds[agentIndex]*0.1f && vLength > 0) {
						interpolatedVelocities[agentIndex] *= agentMaxSpeeds[agentIndex]*0.1f / vLength;
						interpolatedRotations[agentIndex] = math.lerp(interpolatedRotations[agentIndex], interpolatedVelocities[agentIndex], math.saturate(agentSpeeds[agentIndex] * deltaTime*4f));
					}
				}
			}
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		struct JobGenerateMesh : IJob {
			[ReadOnly] public NativeArray<float2> interpolatedRotations;
			[ReadOnly] public NativeArray<float3> agentPositions;
			[ReadOnly] public NativeArray<float> agentRadii;
			[ReadOnly] public NativeArray<Color> agentColors;

			[WriteOnly] public NativeArray<Vertex> verts;
			[WriteOnly] public NativeArray<int> tris;

			public Vector3 renderingOffset;
			public int startIndex, endIndex;

			public void Execute () {
				for (int i = startIndex; i < endIndex; i++) {
					// Create a square with the "forward" direction along the agent's velocity
					float3 forward = math.normalizesafe(new float3(interpolatedRotations[i].x, 0, interpolatedRotations[i].y)) * agentRadii[i];
					if (math.all(forward == 0)) forward = new float3(0, 0, agentRadii[i]);
					float3 right = math.cross(new float3(0, 1, 0), forward);
					float3 orig = agentPositions[i] + (float3)renderingOffset;

					int vc = 4*i;
					int tc = 2*3*i;

					Color32 color = agentColors[i];
					verts[vc+0] = new Vertex {
						position = (orig + forward - right),
						uv = new float2(0, 1),
						color = color,
					};

					verts[vc+1] = new Vertex {
						position = (orig + forward + right),
						uv = new float2(1, 1),
						color = color,
					};

					verts[vc+2] = new Vertex {
						position = (orig - forward + right),
						uv = new float2(1, 0),
						color = color,
					};

					verts[vc+3] = new Vertex {
						position = (orig - forward - right),
						uv = new float2(0, 0),
						color = color,
					};

					tris[tc+0] = (vc + 0);
					tris[tc+1] = (vc + 1);
					tris[tc+2] = (vc + 2);

					tris[tc+3] = (vc + 0);
					tris[tc+4] = (vc + 2);
					tris[tc+5] = (vc + 3);
				}
			}
		}

		[BurstCompile]
		struct JobSetAgentSettings : IJob {
			public SimulatorBurst.AgentData data;
			[ReadOnly] public NativeArray<float3> goals;
			public float radius;
			public float agentTimeHorizon;
			public float obstacleTimeHorizon;
			public int maxNeighbours;
			public bool debug;
			public float maxSpeed;
			public int startIndex, endIndex;

			public void Execute () {
				for (int i = startIndex; i < endIndex; i++) {
					data.radius[i] = radius;
					data.agentTimeHorizon[i] = agentTimeHorizon;
					data.obstacleTimeHorizon[i] = obstacleTimeHorizon;
					data.maxNeighbours[i] = maxNeighbours;
					data.debugDraw[i] = i == 0 && debug;

					// Set the desired velocity for the agent
					var dist = math.length(goals[i] - data.position[i]);
					data.SetTarget(i, goals[i], math.min(dist, maxSpeed), maxSpeed*1.1f);

					// Decrease agent priority when it is close to its goal.
					// Also protect against division by zero.
					data.priority[i] = math.max(0.1f, 1 - math.exp(-0.5f * dist / math.max(maxSpeed, 0.1f)));
				}
			}
		}

		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		struct Vertex {
			public float3 position;
			public Color32 color;
			public float2 uv;
		}

		public void Update () {
			if (agents == null || mesh == null) return;

			var burstSim = sim as SimulatorBurst;
			if (burstSim == null) throw new System.Exception("The Lightweight RVO example requires the burst option to be enabled on the RVOSimulator");

			int agentCount = burstSim.AgentCount;

			if (agents.Count != agentCount) {
				Debug.LogError("Agent count does not match");
				return;
			}

			Profiler.BeginSample("Schedule");

			// Make sure the arrays are large enough
			if (interpolatedVelocities.Length != agentCount) {
				Util.Memory.Realloc(ref interpolatedVelocities, agentCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
				Util.Memory.Realloc(ref interpolatedRotations, agentCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);

				var layout = new[] {
					new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
					new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
					new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
				};
				var vertexCount = agentCount*4;
				mesh.SetVertexBufferParams(vertexCount, layout);
				// To allow for more than ≈16k agents we need to use a 32 bit format for the mesh
				mesh.SetIndexBufferParams(agentCount*6, IndexFormat.UInt32);
			}

			NativeArray<Vertex> nativeVerts = new NativeArray<Vertex>(agentCount*4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> nativeTris = new NativeArray<int>(agentCount*6, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

			var settingsJob = new JobSetAgentSettings {
				data = burstSim.simulationData,
				goals = goals,
				radius = radius,
				agentTimeHorizon = agentTimeHorizon,
				obstacleTimeHorizon = obstacleTimeHorizon,
				maxNeighbours = maxNeighbours,
				debug = debug,
				maxSpeed = maxSpeed,
				startIndex = 0,
				endIndex = agentCount,
			}.Schedule();

			var moveJob = new JobMoveAgents {
				interpolatedVelocities = interpolatedVelocities,
				interpolatedRotations = interpolatedRotations,
				agentPositions = burstSim.simulationData.position,
				agentMaxSpeeds = burstSim.simulationData.maxSpeed,
				agentTargetPoints = burstSim.outputData.targetPoint,
				agentSpeeds = burstSim.outputData.speed,
				deltaTime = Time.deltaTime,
				startIndex = 0,
				endIndex = agentCount,
				debug = debug,
			}.Schedule(settingsJob);

			var meshJob = new JobGenerateMesh {
				interpolatedRotations = interpolatedRotations,
				agentPositions = burstSim.simulationData.position,
				agentRadii = burstSim.simulationData.radius,
				agentColors = agentColors,
				verts = nativeVerts,
				//uvs = nativeUVs,
				//colors = nativeColors,
				tris = nativeTris,
				startIndex = 0,
				endIndex = agentCount,
				renderingOffset = renderingOffset,
			}.Schedule(moveJob);

			UnityEngine.Profiling.Profiler.EndSample();

			meshJob.Complete();

			// Update the mesh data
			mesh.SetVertexBufferData(nativeVerts, 0, 0, nativeVerts.Length);
			mesh.SetIndexBufferData(nativeTris, 0, 0, nativeTris.Length);

			mesh.subMeshCount = 1;
			mesh.SetSubMesh(0, new SubMeshDescriptor(0, nativeTris.Length, MeshTopology.Triangles), MeshUpdateFlags.DontRecalculateBounds);
			// SetSubMesh doesn't seem to update the bounds properly for some reason, so we do it manually instead
			mesh.RecalculateBounds();

			nativeVerts.Dispose();
			nativeTris.Dispose();
		}
	}
}
