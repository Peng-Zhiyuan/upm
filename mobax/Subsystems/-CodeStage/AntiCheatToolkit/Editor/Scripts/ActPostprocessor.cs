#define DEBUG
#undef DEBUG

#define DEBUG_VERBOSE
#undef DEBUG_VERBOSE

#define DEBUG_PARANIOD
#undef DEBUG_PARANIOD

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEditor.Callbacks;
using Debug = UnityEngine.Debug;

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
using System.Diagnostics;
#endif

namespace CodeStage.AntiCheat.EditorCode
{
	internal class ActPostprocessor:AssetPostprocessor
	{
		private static readonly List<AllowedAssembly> allowedAssemblies = new List<AllowedAssembly>();
		private static readonly List<string> allLibraries = new List<string>();

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
		[UnityEditor.MenuItem("Anti-Cheat Toolkit/Force Injection Detector data collection")]
		private static void CallInjectionScan()
		{
			InjectionAssembliesScan(true); 
		}
#endif

		// called by Unity
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (!EditorPrefs.GetBool(ActEditorGlobalStuff.PREFS_INJECTION_ENABLED)) return;
			if (!IsInjectionDetectorTargetCompatible())
			{
				InjectionDetectorTargetCompatibleCheck();
				return;
			}

			if (deletedAssets.Length > 0)
			{
				foreach (string deletedAsset in deletedAssets)
				{
					if (deletedAsset.IndexOf(ActEditorGlobalStuff.INJECTION_DATA_FILE) > -1 && !EditorApplication.isCompiling)
					{
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
						Debug.LogWarning("Looks like Injection Detector data file was accidentally removed! Re-creating...\nIf you wish to remove " + ActEditorGlobalStuff.INJECTION_DATA_FILE + " file, just disable Injection Detecotr in the ACTk Settings window.");
#endif
						InjectionAssembliesScan();
					}
				}
			}
		}

		// called by Unity
		[DidReloadScripts]
		private static void ScriptsWereReloaded()
		{
			EditorUserBuildSettings.activeBuildTargetChanged += OnBuildTargetChanged;

			if (EditorPrefs.GetBool(ActEditorGlobalStuff.PREFS_INJECTION_ENABLED))
			{
				InjectionAssembliesScan();
			}
		}

		private static void OnBuildTargetChanged()
		{
			InjectionDetectorTargetCompatibleCheck();
		}

		internal static void InjectionAssembliesScan()
		{
			InjectionAssembliesScan(false);
		}

		internal static void InjectionAssembliesScan(bool forced)
		{
			if (!IsInjectionDetectorTargetCompatible() && !forced)
			{
				InjectionDetectorTargetCompatibleCheck();
				return;
			}

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			Stopwatch sw = Stopwatch.StartNew();
	#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Injection Detector Assemblies Scan\n");
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Paths:\n" +

			          "Assets: " + ActEditorGlobalStuff.ASSETS_PATH + "\n" +
			          "Assemblies: " + ActEditorGlobalStuff.ASSEMBLIES_PATH + "\n" +
			          "Injection Detector Data: " + ActEditorGlobalStuff.INJECTION_DATA_PATH);
			sw.Start();
	#endif
#endif

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Looking for all assemblies in current project...");
			sw.Start();
#endif
			allLibraries.Clear();
			allowedAssemblies.Clear();

			allLibraries.AddRange(ActEditorGlobalStuff.FindLibrariesAt(ActEditorGlobalStuff.assetsPath));
			allLibraries.AddRange(ActEditorGlobalStuff.FindLibrariesAt(ActEditorGlobalStuff.assembliesPath));
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Total libraries found: " + allLibraries.Count);
			sw.Start();
#endif
			const string editorSubdir = "/editor/";
			string assembliesPathLowerCase = ActEditorGlobalStuff.ASSEMBLIES_PATH_RELATIVE.ToLower();
			foreach (string libraryPath in allLibraries)
			{
				string libraryPathLowerCase = libraryPath.ToLower();
#if (DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Checking library at the path: " + libraryPathLowerCase);
				sw.Start();
#endif
				if (libraryPathLowerCase.Contains(editorSubdir)) continue;
				if (libraryPathLowerCase.Contains("-editor.dll") && libraryPathLowerCase.Contains(assembliesPathLowerCase)) continue;

				try
				{
					AssemblyName assName = AssemblyName.GetAssemblyName(libraryPath);
					string name = assName.Name;
					int hash = ActEditorGlobalStuff.GetAssemblyHash(assName);

					AllowedAssembly allowed = allowedAssemblies.FirstOrDefault(allowedAssembly => allowedAssembly.name == name);

					if (allowed != null)
					{
						allowed.AddHash(hash);
					}
					else
					{
						allowed = new AllowedAssembly(name, new[] {hash});
						allowedAssemblies.Add(allowed);
					}
				}
				catch
				{
					// not a valid IL assembly, skipping
				}
			}

#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			string trace = ActEditorGlobalStuff.LOG_PREFIX + "Found assemblies (" + allowedAssemblies.Count + "):\n";

			foreach (AllowedAssembly allowedAssembly in allowedAssemblies)
			{
				trace += "  Name: " + allowedAssembly.name + "\n";
				trace = allowedAssembly.hashes.Aggregate(trace, (current, hash) => current + ("    Hash: " + hash + "\n"));
			}

			Debug.Log(trace);
			sw.Start();
#endif
			if (!Directory.Exists(ActEditorGlobalStuff.resourcesPath))
			{
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Creating resources folder: " + ActEditorGlobalStuff.RESOURCES_PATH);
				sw.Start();
#endif
				Directory.CreateDirectory(ActEditorGlobalStuff.resourcesPath);
			}

			ActEditorGlobalStuff.RemoveReadOnlyAttribute(ActEditorGlobalStuff.injectionDataPath);
			BinaryWriter bw = new BinaryWriter(new FileStream(ActEditorGlobalStuff.injectionDataPath, FileMode.Create, FileAccess.Write, FileShare.Read));
			int allowedAssembliesCount = allowedAssemblies.Count;

			int totalWhitelistedAssemblies;

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Processing default whitelist");
			sw.Start();
#endif

			string defaultWhitelistPath = ActEditorGlobalStuff.ResolveInjectionDefaultWhitelistPath();
			if (File.Exists(defaultWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(defaultWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();
				totalWhitelistedAssemblies = assembliesCount + allowedAssembliesCount;

				bw.Write(totalWhitelistedAssemblies);

				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}
			else
			{
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
				sw.Stop();
#endif
				bw.Close();
				Debug.LogError(ActEditorGlobalStuff.LOG_PREFIX + "Can't find " + ActEditorGlobalStuff.INJECTION_DEFAULT_WHITELIST_FILE + " file!\nPlease, report to " + ActEditorGlobalStuff.REPORT_EMAIL);
				return;
			}

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Processing user whitelist");
			sw.Start();
#endif

			string userWhitelistPath = ActEditorGlobalStuff.ResolveInjectionUserWhitelistPath();
			if (File.Exists(userWhitelistPath))
			{
				BinaryReader br = new BinaryReader(new FileStream(userWhitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				int assembliesCount = br.ReadInt32();

				bw.Seek(0, SeekOrigin.Begin);
				bw.Write(totalWhitelistedAssemblies + assembliesCount);
				bw.Seek(0, SeekOrigin.End);
				for (int i = 0; i < assembliesCount; i++)
				{
					bw.Write(br.ReadString());
				}
				br.Close();
			}

#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Processing project assemblies");
			sw.Start();
#endif

			for (int i = 0; i < allowedAssembliesCount; i++)
			{
				AllowedAssembly assembly = allowedAssemblies[i];
				string name = assembly.name;
				string hashes = "";

				for (int j = 0; j < assembly.hashes.Length; j++)
				{
					hashes += assembly.hashes[j];
					if (j < assembly.hashes.Length - 1)
					{
						hashes += ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR;
					}
				}

				string line = ObscuredString.EncryptDecrypt(name + ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR + hashes, "Elina");
				
#if (DEBUG_VERBOSE || DEBUG_PARANIOD)
				Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Writing assembly:\n" + name + ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR + hashes);
#endif
				bw.Write(line);
			}

			bw.Close();			 
#if (DEBUG || DEBUG_VERBOSE || DEBUG_PARANIOD)
			sw.Stop();
			Debug.Log(ActEditorGlobalStuff.LOG_PREFIX + "Assemblies scan duration: " + sw.ElapsedMilliseconds + " ms.");
#endif

			if (allowedAssembliesCount == 0)
			{
				Debug.LogError(ActEditorGlobalStuff.LOG_PREFIX + "Can't find any assemblies!\nPlease, report to " + ActEditorGlobalStuff.REPORT_EMAIL);
			}

			AssetDatabase.Refresh();
			//EditorApplication.UnlockReloadAssemblies();
		}

		public static bool IsInjectionDetectorTargetCompatible()
		{
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_ANDROID
			return true;
#else
			return false;
#endif
		}

		private static void InjectionDetectorTargetCompatibleCheck()
		{
			if (!IsInjectionDetectorTargetCompatible())
			{
				if (!File.Exists(ActEditorGlobalStuff.injectionDataPath)) return;
				Debug.LogWarning(ActEditorGlobalStuff.LOG_PREFIX + "Injection Detector is not available on selected platform (" + EditorUserBuildSettings.activeBuildTarget + ") and will be disabled!");
				ActEditorGlobalStuff.CleanInjectionDetectorData();
			}
		}
    }
}