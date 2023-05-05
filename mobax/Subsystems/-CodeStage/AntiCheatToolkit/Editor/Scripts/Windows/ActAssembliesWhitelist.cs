using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.Windows
{
	internal class ActAssembliesWhitelist : EditorWindow
	{
		private const string INITIAL_CUSTOM_NAME = "AssemblyName, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
		private static List<AllowedAssembly> whitelist;
		private static string whitelistPath;

		private Vector2 scrollPosition;
		private bool manualAssemblyWhitelisting;
		private string manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;

		[UnityEditor.MenuItem(ActEditorGlobalStuff.WINDOWS_MENU_PATH + "Injection Detector Whitelist Editor...")]
		internal static void ShowWindow()
		{
			EditorWindow myself = GetWindow<ActAssembliesWhitelist>(false, "Whitelist Editor", true);
			myself.minSize = new Vector2(500, 200);
		}

		private void OnLostFocus()
		{
			manualAssemblyWhitelisting = false;
			manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
		}

		private void OnGUI()
		{
			if (whitelist == null)
			{
				whitelist = new List<AllowedAssembly>();
				LoadAndParseWhitelist();
			}

			GUIStyle tmpStyle = new GUIStyle(EditorStyles.largeLabel);
			tmpStyle.alignment = TextAnchor.MiddleCenter;
			tmpStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label("User-defined Whitelist of Assemblies trusted by Injection Detector", tmpStyle);

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			bool whitelistUpdated = false;

			int count = whitelist.Count;

			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					AllowedAssembly assembly = whitelist[i];
					GUILayout.BeginHorizontal();
					GUILayout.Label(assembly.ToString());
					if (GUILayout.Button(new GUIContent("-", "Remove Assembly from Whitelist"), GUILayout.Width(30)))
					{
						whitelist.Remove(assembly);
						whitelistUpdated = true;
					}
					GUILayout.EndHorizontal();
				}
			}
			else
			{
				tmpStyle = new GUIStyle(EditorStyles.largeLabel);
				tmpStyle.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label("- no Assemblies added so far (use buttons below to add) -", tmpStyle);
			}

			if (manualAssemblyWhitelisting)
			{
				manualAssemblyWhitelistingName = EditorGUILayout.TextField(manualAssemblyWhitelistingName);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					try
					{
						AssemblyName assName = new AssemblyName(manualAssemblyWhitelistingName);
						WhitelistingResult res = TryWhitelistAssemblyName(assName, true);
						if (res != WhitelistingResult.Exists)
						{
							whitelistUpdated = true;
						}
						manualAssemblyWhitelisting = false;
						manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
					}
					catch (FileLoadException error)
					{
						ShowNotification(new GUIContent(error.Message));
					}

					GUI.FocusControl("");
				}

				if (GUILayout.Button("Cancel"))
				{
					manualAssemblyWhitelisting = false;
					manualAssemblyWhitelistingName = INITIAL_CUSTOM_NAME;
					GUI.FocusControl("");
				}
				GUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			if (GUILayout.Button("Add Assembly"))
			{
				string assemblyPath = EditorUtility.OpenFilePanel("Choose an Assembly to add", "", "dll");
				if (!string.IsNullOrEmpty(assemblyPath))
				{
					whitelistUpdated |= TryWhitelistAssemblies(new[] { assemblyPath }, true);
				}
			}

			if (GUILayout.Button("Add Assemblies from folder"))
			{
				string selectedFolder = EditorUtility.OpenFolderPanel("Choose a folder with Assemblies", "", "");
				if (!string.IsNullOrEmpty(selectedFolder))
				{
					string[] libraries = ActEditorGlobalStuff.FindLibrariesAt(selectedFolder);
					whitelistUpdated |= TryWhitelistAssemblies(libraries);
				}
			}

			if (!manualAssemblyWhitelisting)
			{
				if (GUILayout.Button("Add Assembly manually"))
				{
					manualAssemblyWhitelisting = true;
				}
			}
			
			if (count > 0)
			{
				if (GUILayout.Button("Clear"))
				{
					if (EditorUtility.DisplayDialog("Please confirm", "Are you sure you wish to completely clear your Injection Detector whitelist?", "Yes", "No"))
					{
						whitelist.Clear();
						whitelistUpdated = true;
					}
				}
			}
			GUILayout.Space(20);
			GUILayout.EndHorizontal();

			GUILayout.Space(20);

			if (whitelistUpdated)
			{
				WriteWhitelist();
			}
		}

		private bool TryWhitelistAssemblies(IList<string> libraries)
		{
			return TryWhitelistAssemblies(libraries, false);
		}

		private bool TryWhitelistAssemblies(IList<string> libraries, bool singleFile)
		{
			int added = 0;
			int updated = 0;

			int count = libraries.Count;

			for (int i = 0; i < count; i++)
			{
				string libraryPath = libraries[i];
				try
				{
					AssemblyName assName = AssemblyName.GetAssemblyName(libraryPath);
					WhitelistingResult whitelistingResult = TryWhitelistAssemblyName(assName, singleFile);
					switch (whitelistingResult)
					{
						case WhitelistingResult.Added:
							added++;
							break;
						case WhitelistingResult.Updated:
							updated++;
							break;
						case WhitelistingResult.Exists:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch
				{
					if (singleFile) ShowNotification(new GUIContent("Selected file is not a valid .NET assembly!"));
				}
			}

			if (!singleFile)
			{
				ShowNotification(new GUIContent("Assemblies added: " + added + ", updated: " + updated));
			}

			return added > 0 || updated > 0;
		}

		private WhitelistingResult TryWhitelistAssemblyName(AssemblyName assName, bool singleFile)
		{
			WhitelistingResult result = WhitelistingResult.Exists;

			string assNameString = assName.Name;
			int hash = ActEditorGlobalStuff.GetAssemblyHash(assName);

			AllowedAssembly allowed = whitelist.FirstOrDefault(allowedAssembly => allowedAssembly.name == assNameString);

			if (allowed != null)
			{
				if (allowed.AddHash(hash))
				{
					if (singleFile) ShowNotification(new GUIContent("New hash added!"));
					result = WhitelistingResult.Updated;
				}
				else
				{
					if (singleFile) ShowNotification(new GUIContent("Assembly already exists!"));
				}
			}
			else
			{
				allowed = new AllowedAssembly(assNameString, new[] {hash});
				whitelist.Add(allowed);

				if (singleFile) ShowNotification(new GUIContent("Assembly added!"));
				result = WhitelistingResult.Added;
			}

			return result;
		}

		private static void LoadAndParseWhitelist()
		{
			whitelistPath = ActEditorGlobalStuff.ResolveInjectionUserWhitelistPath();
			if (string.IsNullOrEmpty(whitelistPath) || !File.Exists(whitelistPath)) return;

			string[] separator = {ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR};

			FileStream fs = new FileStream(whitelistPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BinaryReader br = new BinaryReader(fs);

			int count = br.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				string line = br.ReadString();
				line = ObscuredString.EncryptDecrypt(line, "Elina");
				string[] strArr = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				int stringsCount = strArr.Length;
				if (stringsCount > 1)
				{
					string assemblyName = strArr[0];

					int[] hashes = new int[stringsCount - 1];
					for (int j = 1; j < stringsCount; j++)
					{
						hashes[j - 1] = int.Parse(strArr[j]);
					}

					whitelist.Add(new AllowedAssembly(assemblyName, hashes));
				}
				else
				{
					Debug.LogWarning("Error parsing whitelist file line! Please report to " + ActEditorGlobalStuff.REPORT_EMAIL);
				}
			}

			br.Close();
			fs.Close();
		}

		private static void WriteWhitelist()
		{
			if (whitelist.Count > 0)
			{
				bool fileExisted = File.Exists(whitelistPath);
				ActEditorGlobalStuff.RemoveReadOnlyAttribute(whitelistPath);
				FileStream fs = new FileStream(whitelistPath, FileMode.Create, FileAccess.Write, FileShare.Read);
				BinaryWriter br = new BinaryWriter(fs);

				br.Write(whitelist.Count);

				foreach (AllowedAssembly assembly in whitelist)
				{
					string assemblyName = assembly.name;
					string hashes = "";

					for (int j = 0; j < assembly.hashes.Length; j++)
					{
						hashes += assembly.hashes[j];
						if (j < assembly.hashes.Length - 1)
						{
							hashes += ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR;
						}
					}

					string line = ObscuredString.EncryptDecrypt(assemblyName + ActEditorGlobalStuff.INJECTION_DATA_SEPARATOR + hashes, "Elina");
					br.Write(line);
				}
				br.Close();
				fs.Close();

				if (!fileExisted)
				{
					AssetDatabase.Refresh();
				}
			}
			else
			{
				ActEditorGlobalStuff.RemoveReadOnlyAttribute(whitelistPath);
				FileUtil.DeleteFileOrDirectory(whitelistPath);
				AssetDatabase.Refresh();
			}

			ActPostprocessor.InjectionAssembliesScan();
		}

		//////////////////////////////////////

		private enum WhitelistingResult : byte
		{
			Exists,
			Added,
			Updated
		}
	}
}