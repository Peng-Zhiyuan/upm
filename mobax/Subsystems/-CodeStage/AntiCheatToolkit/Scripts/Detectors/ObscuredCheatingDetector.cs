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
	/// Detects cheating of any Obscured type (except \link ObscuredTypes.ObscuredPrefs ObscuredPrefs\endlink, it has own detection features) used in project.
	/// </summary>
	/// It allows cheaters to find desired (fake) values in memory and change them, keeping original values secure.<br/>
	/// It's like a cheese in the mouse trap - cheater tries to change some obscured value and get caught on it.
	/// 
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit" 
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	/// 
	/// Avoid using detectors from code at the Awake phase.
	[AddComponentMenu(MENU_PATH + COMPONENT_NAME)]
	public class ObscuredCheatingDetector : ActDetectorBase
	{
		internal const string COMPONENT_NAME = "Obscured Cheating Detector";
		internal const string FINAL_LOG_PREFIX = Constants.LOG_PREFIX + COMPONENT_NAME + ": ";

		private static int instancesInScene;

		#region public fields
		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredFloat ObscuredFloat\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredFloat. Increase in case of false positives.")]
		public float floatEpsilon = 0.0001f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredVector2 ObscuredVector2\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector2. Increase in case of false positives.")]
		public float vector2Epsilon = 0.1f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredVector3 ObscuredVector3\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredVector3. Increase in case of false positives.")]
		public float vector3Epsilon = 0.1f;

		/// <summary>
		/// Max allowed difference between encrypted and fake values in \link ObscuredTypes.ObscuredQuaternion ObscuredQuaternion\endlink. Increase in case of false positives.
		/// </summary>
		[Tooltip("Max allowed difference between encrypted and fake values in ObscuredQuaternion. Increase in case of false positives.")]
		public float quaternionEpsilon = 0.1f;
		#endregion

		#region public static methods
		/// <summary>
		/// Starts all Obscured types cheating detection.
		/// </summary>
		/// Make sure you have properly configured detector in scene with #autoStart disabled before using this method.
		public static void StartDetection()
		{
			if (Instance != null)
			{
				Instance.StartDetectionInternal(null);
			}
			else
			{
				Debug.LogError(FINAL_LOG_PREFIX + "can't be started since it doesn't exists in scene or not yet initialized!");
			}
		}

		/// <summary>
		/// Starts all Obscured types cheating detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="callback">Method to call after detection.</param>
		public static void StartDetection(UnityAction callback)
		{
			GetOrCreateInstance.StartDetectionInternal(callback);
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
		public static ObscuredCheatingDetector Instance { get; private set; }

		private static ObscuredCheatingDetector GetOrCreateInstance
		{
			get
			{
			    if (Instance != null) return Instance;

			    if (detectorsContainer == null)
			    {
			        detectorsContainer = new GameObject(CONTAINER_NAME);
			    }
			    Instance = detectorsContainer.AddComponent<ObscuredCheatingDetector>();
			    return Instance;
			}
		}
		#endregion

		internal static bool IsRunning
		{
			get
			{
				//object.Equals(Instance, null); 
                return ((object)Instance != null) && Instance.isRunning;
			}
		}

		private ObscuredCheatingDetector() {} // prevents direct instantiation

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
		#endregion

		private void StartDetectionInternal(UnityAction callback)
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
			started = true;
			isRunning = true;
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null);
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
    }
}