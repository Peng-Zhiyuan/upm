#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System.IO;
using System.Security.Cryptography;

namespace AK.Wwise.Unity.WwiseAddressables
{
	[UnityEditor.AssetImporters.ScriptedImporter(1, "wem")]
	public class WwiseStreamingAssetImporter : UnityEditor.AssetImporters.ScriptedImporter
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

			WwiseStreamingMediaAsset dataAsset = ScriptableObject.CreateInstance<WwiseStreamingMediaAsset>();
			dataAsset.RawData = File.ReadAllBytes(Path.GetFullPath(ctx.assetPath));
			byte[] hash = MD5.Create().ComputeHash(dataAsset.RawData);
			dataAsset.hash = hash;

			ctx.AddObjectToAsset(string.Format("WwiseSteamingMedia_{0}{1}_{2}", platform, language, assetName), dataAsset);
			ctx.SetMainObject(dataAsset);
		}
	}
}
#endif