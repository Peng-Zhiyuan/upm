#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES

using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AK.Wwise.Unity.WwiseAddressables
{
	[CreateAssetMenu(fileName = "BuildScriptWwisePacked.asset", menuName = "Addressables/Content Builders/Wwise Build Script")]
	public class BuildScriptWwisePacked : BuildScriptPackedMode
	{
		/// <inheritdoc />
		public override string Name
		{
			get
			{
				return "Wwise Build Script";
			}
		}

		AddressableAssetsBuildContext lastAaContext;

		/// <inheritdoc />
		protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
		{
			if (assetGroup == null)
				return string.Empty;

			// pzy:
			// 同一次构建指需要处理一次 group 
			if(lastAaContext != aaContext)
			{
				lastAaContext = aaContext;
				var buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), aaContext.runtimeData.BuildTarget);
				IncludePlatformSpecificBundles(buildTarget);
			}


			foreach (var schema in assetGroup.Schemas)
			{
				var errorString = ProcessGroupSchema(schema, assetGroup, aaContext);
				if (!string.IsNullOrEmpty(errorString))
					return errorString;
			}

			return string.Empty;
		}

		private void IncludePlatformSpecificBundles(UnityEditor.BuildTarget target)
		{
			// pzy:
			// 这里代码有问题
			// AkAssetUtilities.GetWwisePlatformName 会返回编辑器运行的平台，而非指定构建的平台
			// 替换掉代码
			//var wwisePlatform = AkAssetUtilities.GetWwisePlatformName(target);

			string wwisePlatform = "";
			if(target == BuildTarget.Android)
            {
				wwisePlatform = "Android";
			}
			else if(target == BuildTarget.iOS) 
            {
				wwisePlatform = "iOS";
            }
			else if(target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
				wwisePlatform = "Windows";
			}
			else if(target == BuildTarget.StandaloneOSX)
            {
				wwisePlatform = "Mac";
			}
			else
            {
				throw new Exception("[BuildScriptWwidePacked] not support build target platform: " + target);
            }


			var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;

			if (addressableSettings == null)
			{
				UnityEngine.Debug.LogWarningFormat("[Addressables] settings file not found.\nPlease go to Menu/Window/Asset Management/Addressables/Groups, then click 'Create Addressables Settings' button.");
				return;
			}

			foreach (var group in addressableSettings.groups)
			{
				var include = true;
				if (group.Name.Contains("WwiseData")){
					if (!group.Name.Contains(wwisePlatform) && !group.name.Contains("AddressableSoundbanks"))
					{
						include = false;
					}
					Debug.Log("[BuildScriptWwisePacked] group: " + group.Name + " set include: " + include);
					var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
					if (bundleSchema != null)
					{
						bundleSchema.IncludeInBuild = include;
					}

						
				}
			}
		}
	}
}
#endif