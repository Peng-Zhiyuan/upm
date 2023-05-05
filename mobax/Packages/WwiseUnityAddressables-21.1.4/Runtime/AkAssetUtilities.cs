﻿#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Unity.WwiseAddressables
{
	public static class AkAssetUtilities
	{
#if UNITY_EDITOR
		public delegate bool AddressableBankCreatedDelegate(WwiseAddressableSoundBank assetRef, string name);
		public static AddressableBankCreatedDelegate AddressableBankUpdated;
		public static Dictionary<string, PlatformEntry> soundbanksInfo = new Dictionary<string, PlatformEntry>();

		public class SoundBankInfo
		{
			public List<SoundBankStreamedFile> streamedFiles = new List<SoundBankStreamedFile>();
			public List<string> events = new List<string>();
		}

		public class SoundBankEntry : Dictionary<string, SoundBankInfo>
		{
		}

		public class PlatformEntry : Dictionary<string, SoundBankEntry>
		{
			public long lastParseTime;
			public Dictionary<string, List<string>> eventToSoundBankMap = new Dictionary<string, List<string>>();

		}


		public struct SoundBankStreamedFile
		{
			public string id;
			public string path;
		}

		public static string GetWwisePlatformName(BuildTarget platform)
		{
			return AkBasePathGetter.GetPlatformName();
		}

		public static void ParseAssetPath(string assetPath, out string platform, out string language)
		{
			platform = string.Empty;
			language = "SFX";

			var banksPath = $"{GetSoundbanksPath()}/";
			var assetRelPath = assetPath.Replace(banksPath, "");

			string[] parts = assetRelPath.Split('/');
			platform = parts[0];

			if (parts.Length > 2)
			{
				language = parts[1];
			}
		}

		public static string GetSoundbanksPath()
		{
			if (AkWwiseEditorSettings.Instance.GeneratedSoundbanksPath == null)
			{
				UnityEngine.Debug.LogError("Wwise Addressables: You need to set the GeneratedSoundbankPath in the Wwise Editor settings or assets will not be properly imported.");
				return string.Empty;
			}
			var path = Path.Combine("Assets", AkWwiseEditorSettings.Instance.GeneratedSoundbanksPath);
			return path.Replace("\\", "/");
		}

		public static void ClearSoundbankInfo()
		{
			soundbanksInfo.Clear();
		}

		public static void AddSoundBank(string bankName, string bankLanguage, ref PlatformEntry soundBankDict)
		{
			if (!soundBankDict.ContainsKey(bankName))
			{
				soundBankDict.Add(bankName, new SoundBankEntry());
			}
			soundBankDict[bankName][bankLanguage] = new SoundBankInfo();
		}


		//Parse soundbank xml file to get a dict of the streaming wem files
		public static PlatformEntry ParsePlatformSoundbanksXML(string platformName, string newBankName)
		{
			if (platformName == null)
			{
				platformName = AkBasePathGetter.GetPlatformName();
			}

			if (!AkBasePathGetter.GetSoundBankPaths(platformName, out string sourceFolder, out string destinationFolder))
			{
				Debug.LogError($"Could not find containing folder for {newBankName} soundbank - platform: {platformName}. Check the Generated Soundbanks Path in the Unity Wwise project settings.");
				return null;
			}

			var xmlFilename = Path.Combine(sourceFolder, "SoundbanksInfo.xml");
			if (!File.Exists(xmlFilename))
			{
				Debug.LogError($"Could not find SoundbanksInfo for {platformName} platform. Check the Generated Soundbanks Path in the Unity Wwise project settings.");
				return null;
			}
			bool doParse = false;
			if (!soundbanksInfo.ContainsKey(platformName))
			{
				doParse = true;
			}
			else if (soundbanksInfo.ContainsKey(platformName) && !soundbanksInfo[platformName].ContainsKey(newBankName))
			{
				doParse = true;
			}
			else
			{
				var fileModifiedTime = System.IO.File.GetLastWriteTime(xmlFilename);
				if (fileModifiedTime.Ticks > soundbanksInfo[platformName].lastParseTime)
				{
					doParse = true;
				}
			}

			if (doParse)
			{
				var soundBanks = new PlatformEntry();
				soundBanks.lastParseTime = DateTime.Now.Ticks;
				var doc = new System.Xml.XmlDocument();
				doc.Load(xmlFilename);

				var soundBanksRootNode = doc.GetElementsByTagName("SoundBanks");
				for (var i = 0; i < soundBanksRootNode.Count; i++)
				{
					var soundBankNodes = soundBanksRootNode[i].SelectNodes("SoundBank");
					for (var j = 0; j < soundBankNodes.Count; j++)
					{
						var bankName = soundBankNodes[j].SelectSingleNode("ShortName").InnerText;
						var language = soundBankNodes[j].Attributes.GetNamedItem("Language").Value;

						AddSoundBank(bankName, language, ref soundBanks);

						if (bankName.Equals("Init"))
						{
							continue;
						}

						var includedEventsNode = soundBankNodes[j].SelectSingleNode("IncludedEvents");
						if (includedEventsNode != null)
						{
							var eventNodes = includedEventsNode.SelectNodes("Event");
							for (var e = 0; e < eventNodes.Count; e++)
							{
								soundBanks[bankName][language].events.Add(eventNodes[e].Attributes["Name"].Value);

								var streamedFilesRootNode = eventNodes[e].SelectSingleNode("ReferencedStreamedFiles");
								if (streamedFilesRootNode != null)
								{
									var streamedFileNodes = streamedFilesRootNode.SelectNodes("File");
									if (streamedFileNodes.Count > 0)
									{
										for (var s = 0; s < streamedFileNodes.Count; s++)
										{
											var streamedFilelanguage = streamedFileNodes[s].Attributes.GetNamedItem("Language").Value;

											if (!soundBanks[bankName].ContainsKey(streamedFilelanguage))
											{
												AddSoundBank(bankName, streamedFilelanguage, ref soundBanks);
											}
											var streamedFile = new SoundBankStreamedFile
											{
												id = streamedFileNodes[s].Attributes["Id"].Value,
												path = Path.GetFileName(streamedFileNodes[s].SelectSingleNode("Path").InnerText)
											};

											soundBanks[bankName][streamedFilelanguage].streamedFiles.Add(streamedFile);
											if (!soundBanks.eventToSoundBankMap.ContainsKey(streamedFile.id))
											{
												soundBanks.eventToSoundBankMap[streamedFile.id] = new List<string>();
											}
											soundBanks.eventToSoundBankMap[streamedFile.id].Add(bankName);
										}
									}
								}
							}
						}
					}
				}
				soundbanksInfo[platformName] = soundBanks;
			}
			return soundbanksInfo[platformName];
		}

		public static PlatformEntry ParseEditorPlatformSoundbanksXML(string platformName, string newBankName, string sourceFolder) 
		{
			var xmlFilename = Path.Combine(sourceFolder, "SoundbanksInfo.xml");
			if (!File.Exists(xmlFilename))
			{
				Debug.LogError($"Could not find SoundbanksInfo for {platformName} platform. Check the Generated Soundbanks Path in the Unity Wwise project settings.");
				return null;
			}
			bool doParse = false;
			if (!soundbanksInfo.ContainsKey(platformName))
			{
				doParse = true;
			}
			else if (soundbanksInfo.ContainsKey(platformName) && !soundbanksInfo[platformName].ContainsKey(newBankName))
			{
				doParse = true;
			}
			else
			{
				var fileModifiedTime = System.IO.File.GetLastWriteTime(xmlFilename);
				if (fileModifiedTime.Ticks > soundbanksInfo[platformName].lastParseTime)
				{
					doParse = true;
				}
			}

			if (doParse)
			{
				var soundBanks = new PlatformEntry();
				soundBanks.lastParseTime = DateTime.Now.Ticks;
				var doc = new System.Xml.XmlDocument();
				doc.Load(xmlFilename);

				var soundBanksRootNode = doc.GetElementsByTagName("SoundBanks");
				for (var i = 0; i < soundBanksRootNode.Count; i++)
				{
					var soundBankNodes = soundBanksRootNode[i].SelectNodes("SoundBank");
					for (var j = 0; j < soundBankNodes.Count; j++)
					{
						var bankName = soundBankNodes[j].SelectSingleNode("ShortName").InnerText;
						var language = soundBankNodes[j].Attributes.GetNamedItem("Language").Value;

						AddSoundBank(bankName, language, ref soundBanks);

						if (bankName.Equals("Init"))
						{
							continue;
						}

						var includedEventsNode = soundBankNodes[j].SelectSingleNode("IncludedEvents");
						if (includedEventsNode != null)
						{
							var eventNodes = includedEventsNode.SelectNodes("Event");
							for (var e = 0; e < eventNodes.Count; e++)
							{
								soundBanks[bankName][language].events.Add(eventNodes[e].Attributes["Name"].Value);

								var streamedFilesRootNode = eventNodes[e].SelectSingleNode("ReferencedStreamedFiles");
								if (streamedFilesRootNode != null)
								{
									var streamedFileNodes = streamedFilesRootNode.SelectNodes("File");
									if (streamedFileNodes.Count > 0)
									{
										for (var s = 0; s < streamedFileNodes.Count; s++)
										{
											var streamedFilelanguage = streamedFileNodes[s].Attributes.GetNamedItem("Language").Value;

											if (!soundBanks[bankName].ContainsKey(streamedFilelanguage))
											{
												AddSoundBank(bankName, streamedFilelanguage, ref soundBanks);
											}
											var streamedFile = new SoundBankStreamedFile
											{
												id = streamedFileNodes[s].Attributes["Id"].Value,
												path = Path.GetFileName(streamedFileNodes[s].SelectSingleNode("Path").InnerText)
											};

											soundBanks[bankName][streamedFilelanguage].streamedFiles.Add(streamedFile);
											if (!soundBanks.eventToSoundBankMap.ContainsKey(streamedFile.id))
											{
												soundBanks.eventToSoundBankMap[streamedFile.id] = new List<string>();
											}
											soundBanks.eventToSoundBankMap[streamedFile.id].Add(bankName);
										}
									}
								}
							}
						}
					}
				}
				soundbanksInfo[platformName] = soundBanks;
			}
			return soundbanksInfo[platformName];
		}

		public static T[] GetAllInstances<T>() where T : ScriptableObject
		{
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
			T[] a = new T[guids.Length];
			for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
			}
			return a;
		}

		public static void FindAndSetBankReference(WwiseAddressableSoundBank addressableBankAsset, string name)
		{
			System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

			var assembly = Assembly.Load("AK.Wwise.Unity.API.WwiseTypes");
			var AkBankReferenceType = assembly.GetType("WwiseBankReference");
			bool success = (bool)AkBankReferenceType.GetMethod("FindBankReferenceAndSetAddressableBank").Invoke(null, new object[] { addressableBankAsset, name });
		}

#endif

		public static bool AreHashesEqual(byte[] existingHash, byte[] newHash)
		{
			if (existingHash == null || newHash == null)
			{
				return false;
			}

			if (existingHash.Length != newHash.Length)
			{
				return false;
			}

			for (int i = 0; i < newHash.Length; i++)
			{
				if (existingHash[i] != newHash[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool UpdateWwiseFileIfNecessary(string wwiseFolder, WwiseAsset asset)
		{
			var hashPath = Path.Combine(wwiseFolder, asset.GetFilename() + ".md5");
			if (File.Exists(hashPath))
			{
				var existingHash = File.ReadAllBytes(hashPath);

				if (!AreHashesEqual(existingHash, asset.hash))
				{
					// Different hash means file content has changed and needs to be updated
					WriteFile(wwiseFolder, hashPath, asset);
					return true;
				}
			}
			else
			{
				// No hash means we are downloading the file for the first time
				WriteFile(wwiseFolder, hashPath, asset);
				return true;
			}
			return false;
		}

		private static void WriteFile(string wwiseFolder, string hashPath, WwiseAsset asset)
		{
			var path = Path.Combine(wwiseFolder, asset.GetFilename());
			File.WriteAllBytes(path, asset.RawData);
			File.WriteAllBytes(hashPath, asset.hash);
		}
	}
}
#endif  // AK_WWISE_ADDRESSABLES
