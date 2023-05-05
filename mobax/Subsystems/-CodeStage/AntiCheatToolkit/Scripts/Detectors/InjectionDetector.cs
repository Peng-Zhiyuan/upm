#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
#if (ACTK_INJECTION_DEBUG || ACTK_INJECTION_DEBUG_VERBOSE || ACTK_INJECTION_DEBUG_PARANOID)
#define DEBUG_NORMAL
#endif

#if (ACTK_INJECTION_DEBUG_VERBOSE || ACTK_INJECTION_DEBUG_PARANOID)
#define DEBUG_VERBOSE
#endif

#if (ACTK_INJECTION_DEBUG_PARANOID)
#define DEBUG_PARANOID
#endif
#endif

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID
#define UNITY_SUPPORTED_PLATFORM
#endif

#define UNITY_5_4_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#undef UNITY_5_4_PLUS
#endif

#if UNITY_5_4_PLUS
using UnityEngine.SceneManagement;
#endif

using System;
using System.IO;
using System.Reflection;
using CodeStage.AntiCheat.Common;
using CodeStage.AntiCheat.ObscuredTypes;
using Debug = UnityEngine.Debug;
using UnityEngine;
using UnityEngine.Events;

#if DEBUG_NORMAL
using System.Diagnostics;
#endif

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Allows to detect foreign managed assemblies in your application.
	/// </summary>
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	/// 
	/// Avoid using detectors from code at the Awake phase.
	/// 
	/// <strong>\htmlonly<font color="7030A0">NOTE #1:</font>\endhtmlonly Make sure you've checked the 
	/// "Enable Injection Detector" option at the "Window > Code Stage > Anti-Cheat Toolkit > Settings" window
	/// before using detector at runtime.<br/>
	/// \htmlonly<font color="7030A0">NOTE #2:</font>\endhtmlonly Always test detector on the
	/// target platform before releasing your application to the public.<br/>
	/// It may detect some external assemblies as foreign,
	/// thus make sure you've added all external assemblies your application uses to the Whitelist (see section 
	/// "How to fill user-defined Whitelist" of the read me for details).<br/>
	/// \htmlonly<font color="7030A0">NOTE #3:</font>\endhtmlonly Disabled in Editor because of specific assemblies causing false positives. Use ACTK_INJECTION_DEBUG symbol to force it in Editor.
	/// 
	/// \htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly Only Standalone, WebPlayer and Android platforms are supported.</strong>
	[AddComponentMenu(MENU_PATH + COMPONENT_NAME)]
	public class InjectionDetector : ActDetectorBase
	{
		internal const string COMPONENT_NAME = "Injection Detector";
		internal const string FINAL_LOG_PREFIX = Constants.LOG_PREFIX + COMPONENT_NAME + ": ";

#if UNITY_SUPPORTED_PLATFORM

		private static int instancesInScene;

		#region private variables
		private bool signaturesAreNotGenuine;
		private AllowedAssembly[] allowedAssemblies;
		private string[] hexTable;
		#endregion

		#region public static methods
		/// <summary>
		/// Starts foreign assemblies injection detection.
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
		/// Starts foreign assemblies injection detection with specified callback.
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
			if (Instance != null)
				Instance.StopDetectionInternal();
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
			if (Instance != null)
				Instance.DisposeInternal();
		}
		#endregion

		#region static instance
		/// <summary>
		/// Allows reaching public properties from code. Can be null.
		/// </summary>
		public static InjectionDetector Instance { get; private set; }

		private static InjectionDetector GetOrCreateInstance
		{
			get
			{
			    if (Instance != null) return Instance;

			    if (detectorsContainer == null)
			    {
			        detectorsContainer = new GameObject(CONTAINER_NAME);
			    }
			    Instance = detectorsContainer.AddComponent<InjectionDetector>();
			    return Instance;
			}
		}
		#endregion

		private InjectionDetector() { } // prevents direct instantiation

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

#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.GetBool("ACTDIDEnabledGlobal", false))
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "is not enabled in Anti-Cheat Toolkit Settings!\nPlease, check readme.pdf for details.", this);
				DisposeInternal();
				return;
			}
#if !DEBUG_NORMAL
			if (Application.isEditor)
			{
				Debug.LogWarning(FINAL_LOG_PREFIX + "does not work in editor (check readme.pdf for details).", this);
				DisposeInternal();
				return;
			}
#else
			Debug.LogWarning(FINAL_LOG_PREFIX + "works in debug mode. There WILL BE false positives in editor, it's fine!", this);
#endif
#endif
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

			if (allowedAssemblies == null)
			{
				LoadAndParseAllowedAssemblies();
			}

			if (signaturesAreNotGenuine)
			{
				OnCheatingDetected();
				return;
			}

			if (!FindInjectionInCurrentAssemblies())
			{
				// listening for new assemblies
				AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
			}
			else
			{
				OnCheatingDetected();
			}
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(null);
		}

		protected override void PauseDetector()
		{
			isRunning = false;
			AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
		}

		protected override void ResumeDetector()
		{
			if (detectionAction == null && !detectionEventHasListener) return;

			isRunning = true;
			AppDomain.CurrentDomain.AssemblyLoad += OnNewAssemblyLoaded;
		}

		protected override void StopDetectionInternal()
		{
			if (!started)
				return;

			AppDomain.CurrentDomain.AssemblyLoad -= OnNewAssemblyLoaded;
			detectionAction = null;
			started = false;
			isRunning = false;
		}

		protected override void DisposeInternal()
		{
			base.DisposeInternal();
			if (Instance == this) Instance = null;
		}

		private void OnNewAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
#if DEBUG_NORMAL
			Debug.Log(Constants.LOG_PREFIX + "New assembly loaded: " + args.LoadedAssembly.FullName, this);
#endif
			if (!AssemblyAllowed(args.LoadedAssembly))
			{
#if DEBUG_NORMAL
				Debug.Log(Constants.LOG_PREFIX + "Injected Assembly found:\n" + args.LoadedAssembly.FullName, this);
#endif
				OnCheatingDetected();
			}
		}

		private bool FindInjectionInCurrentAssemblies()
		{
			bool result = false;
#if DEBUG_NORMAL
			Stopwatch stopwatch = Stopwatch.StartNew();
#endif
			Assembly[] assembliesInCurrentDomain = AppDomain.CurrentDomain.GetAssemblies();
			if (assembliesInCurrentDomain.Length == 0)
			{
#if DEBUG_NORMAL
				stopwatch.Stop();
				Debug.Log(Constants.LOG_PREFIX + "0 assemblies in current domain! Not genuine behavior.", this);
				stopwatch.Start();
#endif
				result = true;
			}
			else
			{
				foreach (Assembly ass in assembliesInCurrentDomain)
				{
#if DEBUG_VERBOSE
				stopwatch.Stop();
				Debug.Log(Constants.LOG_PREFIX + "Currently loaded assembly:\n" + ass.FullName, this);
				stopwatch.Start();
#endif
					if (!AssemblyAllowed(ass))
					{
#if DEBUG_NORMAL
						stopwatch.Stop();
						Debug.Log(Constants.LOG_PREFIX + "Injected Assembly found:\n" + ass.FullName + "\n" + GetAssemblyHash(ass), this);
						stopwatch.Start();
#endif
						result = true;
						break;
					}
				}
			}

#if DEBUG_NORMAL
			stopwatch.Stop();
			Debug.Log(Constants.LOG_PREFIX + "Loaded assemblies scan duration: " + stopwatch.ElapsedMilliseconds + " ms.", this);
#endif
			return result;
		}

		private bool AssemblyAllowed(Assembly ass)
		{
#if !UNITY_WEBPLAYER
			string assemblyName = ass.GetName().Name;
#else
			string fullname = ass.FullName;
			string assemblyName = fullname.Substring(0, fullname.IndexOf(", ", StringComparison.Ordinal));
#endif

			int hash = GetAssemblyHash(ass);
			
			bool result = false;
			for (int i = 0; i < allowedAssemblies.Length; i++)
			{
				AllowedAssembly allowedAssembly = allowedAssemblies[i];

				if (allowedAssembly.name == assemblyName)
				{
					if (Array.IndexOf(allowedAssembly.hashes, hash) != -1)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		private void LoadAndParseAllowedAssemblies()
		{
#if DEBUG_NORMAL
			Debug.Log(Constants.LOG_PREFIX + "Starting LoadAndParseAllowedAssemblies()", this);
			Stopwatch sw = Stopwatch.StartNew();
#endif
			TextAsset assembliesSignatures = (TextAsset)Resources.Load("fndid", typeof(TextAsset));
			if (assembliesSignatures == null)
			{
				signaturesAreNotGenuine = true;
				return;
			}

#if DEBUG_NORMAL
			sw.Stop();
			Debug.Log(Constants.LOG_PREFIX + "Creating separator array and opening MemoryStream", this);
			sw.Start();
#endif

			string[] separator = {":"};

			MemoryStream ms = new MemoryStream(assembliesSignatures.bytes);
			BinaryReader br = new BinaryReader(ms);
			
			int count = br.ReadInt32();

#if DEBUG_NORMAL
			sw.Stop();
			Debug.Log(Constants.LOG_PREFIX + "Allowed assemblies count from MS: " + count, this);
			sw.Start();
#endif

			allowedAssemblies = new AllowedAssembly[count];

			for (int i = 0; i < count; i++)
			{
				string line = br.ReadString();
#if DEBUG_PARANOID
				sw.Stop();
				Debug.Log(Constants.LOG_PREFIX + "Line: " + line, this);
				sw.Start();
#endif
				line = ObscuredString.EncryptDecrypt(line, "Elina");
#if DEBUG_PARANOID
				sw.Stop();
				Debug.Log(Constants.LOG_PREFIX + "Line decrypted : " + line, this);
				sw.Start();
#endif
				string[] strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				int stringsCount = strArr.Length;
#if DEBUG_PARANOID
				sw.Stop();
				Debug.Log(Constants.LOG_PREFIX + "stringsCount : " + stringsCount, this);
				sw.Start();
#endif
				if (stringsCount > 1)
				{
					string assemblyName = strArr[0];

					int[] hashes = new int[stringsCount - 1];
					for (int j = 1; j < stringsCount; j++)
					{
						hashes[j - 1] = int.Parse(strArr[j]);
					}

					allowedAssemblies[i] = new AllowedAssembly(assemblyName, hashes);
				}
				else
				{
					signaturesAreNotGenuine = true;
					br.Close();
					ms.Close();
#if DEBUG_NORMAL
					sw.Stop();
#endif
					return;
				}
			}
			br.Close();
			ms.Close();
			Resources.UnloadAsset(assembliesSignatures);

#if DEBUG_NORMAL
			sw.Stop();
			Debug.Log(Constants.LOG_PREFIX + "Allowed Assemblies parsing duration: " + sw.ElapsedMilliseconds + " ms.", this);
#endif

			hexTable = new string[256];
			for (int i = 0; i < 256; i++)
			{
				hexTable[i] = i.ToString("x2");
			}
		}

		private int GetAssemblyHash(Assembly ass)
		{
			string hashInfo;

#if !UNITY_WEBPLAYER
			AssemblyName assName = ass.GetName();
			byte[] bytes = assName.GetPublicKeyToken();
			if (bytes.Length >= 8)
			{
				hashInfo = assName.Name + PublicKeyTokenToString(bytes);
			}
			else
			{
				hashInfo = assName.Name;
			}
#else
			string fullName = ass.FullName;

			string assemblyName = fullName.Substring(0, fullName.IndexOf(", ", StringComparison.Ordinal));
			int tokenIndex = fullName.IndexOf("PublicKeyToken=", StringComparison.Ordinal) + 15;
			string token = fullName.Substring(tokenIndex, fullName.Length - tokenIndex);
			if (token == "null") token = "";
			hashInfo = assemblyName + token;
#endif

			// based on Jenkins hash function (http://en.wikipedia.org/wiki/Jenkins_hash_function)
			int result = 0;
			int len = hashInfo.Length;

			for (int i = 0; i < len; ++i)
			{
				result += hashInfo[i];
				result += (result << 10);
				result ^= (result >> 6);
			}
			result += (result << 3);
			result ^= (result >> 11);
			result += (result << 15);

			return result;
		}

#if !UNITY_WEBPLAYER
		private string PublicKeyTokenToString(byte[] bytes)
		{
			string result = "";

			// AssemblyName.GetPublicKeyToken() returns 8 bytes
			for (int i = 0; i < 8; i++)
			{
				result += hexTable[bytes[i]];
			}

			return result;
		}
#endif

		private class AllowedAssembly
		{
			public readonly string name;
			public readonly int[] hashes;

			public AllowedAssembly(string name, int[] hashes)
			{
				this.name = name;
				this.hashes = hashes;
			}
		}
#else
		//! @cond
		public static InjectionDetector Instance
		{
			get
			{
				Debug.LogError(FINAL_LOG_PREFIX + "is not supported on selected platform!");
				return null;
			}
		}

		public static void StopDetection()
		{
			Debug.LogError(FINAL_LOG_PREFIX + "is not supported on selected platform!");
		}

		public static void Dispose()
		{
			Debug.LogError(FINAL_LOG_PREFIX + "is not supported on selected platform!");
		}

		public static void StartDetection(UnityAction callback)
		{
			Debug.LogError(FINAL_LOG_PREFIX + "is not supported on selected platform!");
		}

		protected override void PauseDetector()
		{
			
		}

		protected override void ResumeDetector()
		{
			
		}

		protected override void StopDetectionInternal()
		{
			
		}

		protected override void StartDetectionAutomatically()
		{

		}
		//! @endcond
#endif
	}
}