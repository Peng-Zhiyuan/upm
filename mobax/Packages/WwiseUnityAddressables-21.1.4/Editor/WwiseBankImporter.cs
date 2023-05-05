#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES

using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AK.Wwise.Unity.WwiseAddressables;
using UnityEditor.AssetImporters;
namespace AK.Wwise.Unity.WwiseAddressables
{
	[UnityEditor.AssetImporters.ScriptedImporter(1, "bnk")]
	public class WwiseBankImporter : UnityEditor.AssetImporters.ScriptedImporter
	{
		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
		{
			string assetName = Path.GetFileNameWithoutExtension(ctx.assetPath);

			string platform;
			string language;
			AkAssetUtilities.ParseAssetPath(ctx.assetPath, out platform, out language);
			if (platform == null)
			{
				return;
			}

			var soundbankInfos = AkAssetUtilities.ParsePlatformSoundbanksXML(platform, assetName);

			if (!soundbankInfos.ContainsKey(assetName))
			{
				Debug.LogWarning($"Could not properly parse soundbank at {ctx.assetPath} - skipping it.");
				return;
			}
			WwiseSoundBankAsset dataAsset = ScriptableObject.CreateInstance<WwiseSoundBankAsset>();
			dataAsset.RawData = File.ReadAllBytes(Path.GetFullPath(ctx.assetPath));
			var eventNames = soundbankInfos[assetName][language].events;
			if (language != "SFX" && soundbankInfos[assetName].ContainsKey("SFX"))
			{
				eventNames.AddRange(soundbankInfos[assetName]["SFX"].events);
			}
			dataAsset.eventNames = eventNames;
			byte[] hash = MD5.Create().ComputeHash(dataAsset.RawData);
			dataAsset.hash = hash;
			ctx.AddObjectToAsset(string.Format("WwiseBank_{0}{1}_{2}", platform, language, assetName), dataAsset);
			ctx.SetMainObject(dataAsset);
		}

		public static WwiseAddressableSoundBank GenerateWwiseSoundBankAssetEditor(string name)
		{
			string assetName = name;
			string language = "SFX";
			string platform = AkBasePathGetter.GetPlatformName();
			string path = AkBasePathGetter.GetWwiseProjectBankPath();
			string bankPath = Path.Combine(path, assetName);
			if (!File.Exists(bankPath))
			{
				bankPath = Path.Combine(path, "English(US)", assetName);
				language = "English(US)";
				if (!File.Exists(bankPath))
				{
					Debug.Log("Bank:" + name + "not exist");
					return null;
				}
			}
			var soundbankInfos = AkAssetUtilities.ParseEditorPlatformSoundbanksXML(platform, assetName, path);
			WwiseSoundBankAsset dataAsset = ScriptableObject.CreateInstance<WwiseSoundBankAsset>();
			dataAsset.RawData = File.ReadAllBytes(Path.GetFullPath(bankPath));
			if (soundbankInfos == null)
			{
				Debug.LogError("not exist soundbankInfos");
				return null;
			}
			string bankname = Path.GetFileNameWithoutExtension(assetName);
			var eventNames = soundbankInfos[bankname][language].events;
			if (language != "SFX" && soundbankInfos[bankname].ContainsKey("SFX"))
			{
				eventNames.AddRange(soundbankInfos[bankname]["SFX"].events);
			}
			dataAsset.eventNames = eventNames;
			byte[] hash = MD5.Create().ComputeHash(dataAsset.RawData);
			dataAsset.hash = hash;
			dataAsset.name = name;
			WwiseAddressableSoundBank addressableBankAsset = ScriptableObject.CreateInstance<WwiseAddressableSoundBank>();
			addressableBankAsset.UpdateLocalizationLanguages(platform, soundbankInfos[bankname].Keys.ToList());
			addressableBankAsset.AddOrUpdate(platform, language, new AssetReferenceWwiseBankData(AssetDatabase.AssetPathToGUID(bankPath)));
			addressableBankAsset.name = bankname;
			addressableBankAsset.UpdateLocalizationLanguages(platform, soundbankInfos[bankname].Keys.ToList());
			addressableBankAsset.wwisebankData = dataAsset;
			string wenpath = AkBasePathGetter.GetWwiseProjectBankPath();
			List<string> wempathList = new List<string>();
			DirectoryInfo dir = new DirectoryInfo(wenpath);
			foreach (FileInfo file in dir.GetFiles("*.wem", SearchOption.AllDirectories))
			{
				wempathList.Add(file.FullName);
			}

			//AddStreamedAssetsToBanks(wempathList, addressableBankAsset);
			return addressableBankAsset;

		}

		private static void AddStreamedAssetsToBanks(List<string> streamingAssetsAdded, WwiseAddressableSoundBank bank)
		{

			foreach (var assetPath in streamingAssetsAdded)
			{
				WwiseStreamingMediaAsset dataAsset = ScriptableObject.CreateInstance<WwiseStreamingMediaAsset>();
				dataAsset.RawData = File.ReadAllBytes(Path.GetFullPath(assetPath));
				byte[] hash = MD5.Create().ComputeHash(dataAsset.RawData);
				dataAsset.hash = hash;
				bank.mediaEditor.Add(dataAsset);


				//string name = Path.GetFileNameWithoutExtension(assetPath);
				//	string platform = AkBasePathGetter.GetPlatformName();
				//	string language = "SFX";
				//	AkAssetUtilities.ParseAssetPath(assetPath, out platform, out language);
				//	string path = AkBasePathGetter.GetWwiseProjectBankPath();

				//	var soundbankInfos = AkAssetUtilities.ParseEditorPlatformSoundbanksXML(platform, name, path);

				//	var bankNames = soundbankInfos.eventToSoundBankMap[name];
				//	foreach (var bankName in bankNames)
				//	{
				//		var bankAssetDir = Path.GetDirectoryName(assetPath);
				//		if (bankName == bank.name)
				//		{
				//			var bankAsset = bank;
				//			if (!string.IsNullOrEmpty(platform))
				//			{
				//				bankAsset.UpdateLocalizationLanguages(platform, soundbankInfos[bankName].Keys.ToList());
				//				bankAsset.SetStreamingMedia(platform, language, bankAssetDir, soundbankInfos[bankName][language].streamedFiles);
				//			}
				//		}

				//	}
			}
			
		}
	}
}
#endif