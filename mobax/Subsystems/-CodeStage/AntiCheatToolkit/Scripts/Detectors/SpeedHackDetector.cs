#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
#define DEBUG_ENABLED
#endif

#define UNITY_5_4_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#undef UNITY_5_4_PLUS
#endif

#if UNITY_5_4_PLUS
using UnityEngine.SceneManagement;
#endif

using CodeStage.AntiCheat.Common;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Allows to detect Cheat Engine's speed hack (and maybe some other speed hack tools) usage.
	/// </summary>
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	/// 
	/// Avoid using detectors from code at the Awake phase.
	/// 
	/// <strong>\htmlonly<font color="7030A0">NOTE:</font>\endhtmlonly May not detect speed hacks on the rooted devices.</strong>
	[AddComponentMenu(MENU_PATH + COMPONENT_NAME)]
	public class SpeedHackDetector : ActDetectorBase
	{
		internal const string COMPONENT_NAME = "Speed Hack Detector";
		internal const string FINAL_LOG_PREFIX = Constants.LOG_PREFIX + COMPONENT_NAME + ": ";

		private const long TICKS_PER_SECOND = System.TimeSpan.TicksPerMillisecond * 1000;
		private const int THRESHOLD = 5000000; // == 500 ms, allowed time difference between genuine and vulnerable ticks

		private static int instancesInScene;

		#region public fields
		/// <summary> 
		/// Time (in seconds) between detector checks.
		/// </summary>
		[Tooltip("Time (in seconds) between detector checks.")]
		public float interval = 1f;

		/// <summary>
		/// Maximum false positives count allowed before registering speed hack.
		/// </summary>
		[Tooltip("Maximum false positives count allowed before registering speed hack.")]
		public byte maxFalsePositives = 3;

		/// <summary>
		/// Amount of sequential successful checks before clearing internal false positives counter.<br/>
		/// Set 0 to disable Cool Down feature.
		/// </summary>
		[Tooltip("Amount of sequential successful checks before clearing internal false positives counter.\nSet 0 to disable Cool Down feature.")]
		public int coolDown = 30;
		#endregion

		#region private variables
		private byte currentFalsePositives;
		private int currentCooldownShots;
		private long ticksOnStart;
		private long vulnerableTicksOnStart;
		private long prevTicks;
		private long prevIntervalTicks;
		#endregion

		#region public static methods
		/// <summary>
		/// Starts speed hack detection.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static void StartDetection()
		{
			if (Instance != null)
			{
				Instance.StartDetectionInternal(null, Instance.interval, Instance.maxFalsePositives, Instance.coolDown);
			}
			else
			{
				Debug.LogError(FINAL_LOG_PREFIX + "can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		/// <summary>
		/// Starts speed hack detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static void StartDetection(UnityAction callback)
		{
			StartDetection(callback, GetOrCreateInstance.interval);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval.<br/>
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		public static void StartDetection(UnityAction callback, float interval)
		{
			StartDetection(callback, interval, GetOrCreateInstance.maxFalsePositives);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval and maxFalsePositives.<br/>
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides #maxFalsePositives property.</param>
		public static void StartDetection(UnityAction callback, float interval, byte maxFalsePositives)
		{
			StartDetection(callback, interval, maxFalsePositives, GetOrCreateInstance.coolDown);
		}

		/// <summary>
		/// Starts speed hack detection with specified callback using passed interval, maxFalsePositives and coolDown. 
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		/// <param name="interval">Time in seconds between speed hack checks. Overrides #interval property.</param>
		/// <param name="maxFalsePositives">Amount of possible false positives. Overrides #maxFalsePositives property.</param>
		/// <param name="coolDown">Amount of sequential successful checks before resetting false positives counter. Overrides #coolDown property.</param>
		public static void StartDetection(UnityAction callback, float interval, byte maxFalsePositives, int coolDown)
		{
			GetOrCreateInstance.StartDetectionInternal(callback, interval, maxFalsePositives, coolDown);
		}

		/// <summary>
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically 
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}
		#endregion

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code. Can be null.
		/// </summary>
		public static SpeedHackDetector Instance { get; private set; }

		private static SpeedHackDetector GetOrCreateInstance
		{
			get
			{
				if (Instance != null)
					return Instance;

				if (detectorsContainer == null)
				{
					detectorsContainer = new GameObject(CONTAINER_NAME);
				}
				Instance = detectorsContainer.AddComponent<SpeedHackDetector>();
				return Instance;
			}
		}
		#endregion

		private SpeedHackDetector() { } // prevents direct instantiation

		#region unity messages
		private void Awake()
		{
			instancesInScene++;
			if (Init(Instance, COMPONENT_NAME))
			{
				Instance = this;
			}

#if UNITY_5_4_PLUS
			SceneManager.sceneLoaded += OnLevelWasLoadedNew;
#endif
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			instancesInScene--;
		}

#if UNITY_5_4_PLUS
		private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
		{
			OnLevelLoadedCallback();
		}
#else
		private void OnLevelWasLoaded()
		{
			OnLevelLoadedCallback();
		}
#endif

		private void OnLevelLoadedCallback()
		{
			if (instancesInScene < 2)
			{
				if (!keepAlive)
				{
					DisposeInternal();
				}
			}
			else
			{
				if (!keepAlive && Instance != this)
				{
					DisposeInternal();
				}
			}
		}

		private void OnApplicationPause(bool pause)
		{
			if (!pause)
			{
				ResetStartTicks();
			}
		}

		private void Update()
		{
			if (!isRunning)
				return;

			long ticks = System.DateTime.UtcNow.Ticks;
			long ticksSpentSinceLastUpdate = ticks - prevTicks;

			if (ticksSpentSinceLastUpdate < 0 || ticksSpentSinceLastUpdate > TICKS_PER_SECOND)
			{
				ResetStartTicks();
				return;
			}

			prevTicks = ticks;

			long intervalTicks = (long)(interval * TICKS_PER_SECOND);

			if (ticks - prevIntervalTicks >= intervalTicks)
			{
				long vulnerableTicks = System.Environment.TickCount * System.TimeSpan.TicksPerMillisecond;

				if (Mathf.Abs((vulnerableTicks - vulnerableTicksOnStart) - (ticks - ticksOnStart)) > THRESHOLD)
				{
					currentFalsePositives++;
					if (currentFalsePositives > maxFalsePositives)
					{
#if DEBUG_ENABLED
						Debug.LogWarning(Constants.LOG_PREFIX + "SpeedHackDetector: final detection!", this);
#endif
						OnCheatingDetected();
					}
					else
					{
#if DEBUG_ENABLED
						Debug.LogWarning(Constants.LOG_PREFIX + "SpeedHackDetector: detection! Allowed false positives left: " + (maxFalsePositives - currentFalsePositives), this);
#endif
						currentCooldownShots = 0;
						ResetStartTicks();
					}
				}
				else if (currentFalsePositives > 0 && coolDown > 0)
				{
#if DEBUG_ENABLED
					Debug.LogWarning(Constants.LOG_PREFIX + "SpeedHackDetector: success shot! Shots till cool down: " + (coolDown - currentCooldownShots), this);
#endif
					currentCooldownShots++;
					if (currentCooldownShots >= coolDown)
					{
#if DEBUG_ENABLED
						Debug.LogWarning(Constants.LOG_PREFIX + "SpeedHackDetector: cool down!", this);
#endif
						currentFalsePositives = 0;
					}
				}

				prevIntervalTicks = ticks;
			}
		}
		#endregion

		private void StartDetectionInternal(UnityAction callback, float checkInterval, byte falsePositives, int shotsTillCooldown)
		{
			if (isRunning)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "already running!", this);
				return;
			}

			if (!enabled)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "disabled but StartDetection still called from somewhere (see stack trace for this message)!", this);
				return;
			}

			if (callback != null && detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "has properly configured Detection Event in the inspector, but still get started with Action callback. Both Action and Detection Event will be called on detection. Are you sure you wish to do this?", this);
			}

			if (callback == null && !detectionEventHasListener)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "was started without any callbacks. Please configure Detection Event in the inspector, or pass the callback Action to the StartDetection method.", this);
				enabled = false;
				return;
			}

			detectionAction = callback;
			interval = checkInterval;
			maxFalsePositives = falsePositives;
			coolDown = shotsTillCooldown;

			ResetStartTicks();
			currentFalsePositives = 0;
			currentCooldownShots = 0;

			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null, interval, maxFalsePositives, coolDown);
		}
		
		protected override void PauseDetector()
		{
			isRunning = false;
		}

		protected override void ResumeDetector()
		{
			if (detectionAction == null && !detectionEventHasListener) return;
			isRunning = true;
		}

		protected override void StopDetectionInternal()
		{
			if (!started)
				return;

			detectionAction = null;
			started = false;
			isRunning = false;
		}

		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (Instance == this) Instance = null;
		}

		private void ResetStartTicks()
		{
			ticksOnStart = System.DateTime.UtcNow.Ticks;
			vulnerableTicksOnStart = System.Environment.TickCount * System.TimeSpan.TicksPerMillisecond;
			prevTicks = ticksOnStart;
			prevIntervalTicks = ticksOnStart;
		}
	}
}