
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DefaultAssetPostprocessor : IAssetPostprocessor
{
	AssetPostprocessor _processor;
	public DefaultAssetPostprocessor(AssetPostprocessor processor)
	{
		_processor = processor;
	}
	public void PreprocessDefault(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		TextureImporter importer = (TextureImporter)_processor.assetImporter;
		bool isLightMap = AssetImportUtil.Custom_Tex_lightMap(importer.assetPath);
		if (isLightMap)
		{
			PreprocessLightMap(maxSize, textureFormat, halfSize);
			return;
		}
		bool isNormalMap = AssetImportUtil.Custom_Tex_Normal(importer.assetPath);
		if (isNormalMap)
		{
			PreprocessNormalMap(maxSize, textureFormat, halfSize);
			return;
		}

		bool isCubeMap = AssetImportUtil.Custom_Tex_CubeMap(importer.assetPath);
		if (isCubeMap)
		{
			PreprocessCubeMap(maxSize, textureFormat, halfSize);
			return;
		}

		bool isUI = AssetImportUtil.Custom_Tex_UI(importer.assetPath);
		if (isUI)
		{
			PreprocessSprite(maxSize, textureFormat, halfSize);
			return;
		}

		importer.textureType = TextureImporterType.Default;
		importer.textureShape = TextureImporterShape.Texture2D;
		importer.filterMode = FilterMode.Bilinear;
		importer.anisoLevel = 1;

		importer.isReadable = false;
		importer.mipmapEnabled = AssetImportUtil.Custom_Tex_MipMap(importer.assetPath);
		bool isM = AssetImportUtil.Custom_Tex_M(importer.assetPath);
		bool isSplat = AssetImportUtil.Custom_Tex_Splat(importer.assetPath);
		Debug.LogError("Custom_Tex_Splat" + importer.assetPath + "   isSplat:"+ isSplat);
		importer.sRGBTexture = !isM && !isSplat;



		importer.npotScale = TextureImporterNPOTScale.ToNearest;
		TextureImporterPlatformSettings platformSetting = importer.GetDefaultPlatformTextureSettings();
		platformSetting.format = TextureImporterFormat.Automatic;
		if (halfSize)
		{
			maxSize = Mathf.Min(maxSize, CalculateHalfSize(importer));
		}
		platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
		platformSetting.maxTextureSize = maxSize;
	
		platformSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
		platformSetting.textureCompression = TextureImporterCompression.Compressed;
		importer.SetPlatformTextureSettings(platformSetting);
	}

	public void PreprocessSprite(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		TextureImporter importer = (TextureImporter)_processor.assetImporter;
		importer.textureType = TextureImporterType.Sprite;
		importer.filterMode = FilterMode.Bilinear;
		importer.anisoLevel = 1;
		importer.mipmapEnabled = false;
		//importer.npotScale = TextureImporterNPOTScale.ToNearest;
		//importer.isReadable = false;
		importer.sRGBTexture = true;
	
		TextureImporterPlatformSettings platformSetting = importer.GetDefaultPlatformTextureSettings();
		platformSetting.format = TextureImporterFormat.Automatic;
		if (halfSize)
		{
			maxSize = Mathf.Min(maxSize, CalculateHalfSize(importer));
		}
		platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
		platformSetting.maxTextureSize = maxSize;
		
		platformSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
		platformSetting.textureCompression = TextureImporterCompression.Compressed;
		importer.SetPlatformTextureSettings(platformSetting);
	}

	public void PreprocessLightMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		TextureImporter importer = (TextureImporter)_processor.assetImporter;
		bool dirLightMap = AssetImportUtil.Custom_Tex_DirLightMap(importer.assetPath);
		importer.textureType = dirLightMap? TextureImporterType.DirectionalLightmap:TextureImporterType.Lightmap;
		//importer.filterMode = FilterMode.Bilinear;
		//importer.anisoLevel = 1;
		importer.mipmapEnabled = true;
		importer.isReadable = false;
		//importer.streamingMipmaps = true;
		importer.npotScale = TextureImporterNPOTScale.ToNearest;
		//importer.sRGBTexture = true;
		TextureImporterPlatformSettings platformSetting = importer.GetDefaultPlatformTextureSettings();
		platformSetting.format = TextureImporterFormat.Automatic;
		if (halfSize)
		{
			maxSize = Mathf.Min(maxSize, CalculateHalfSize(importer));
		}
		platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
		platformSetting.maxTextureSize = maxSize;
		platformSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
		platformSetting.textureCompression = TextureImporterCompression.Compressed;
		importer.SetPlatformTextureSettings(platformSetting);
	}

	public void PreprocessCubeMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		
		TextureImporter importer = (TextureImporter)_processor.assetImporter;
		Debug.Log("PreprocessCubeMap:" + importer.assetPath);
		importer.textureType = TextureImporterType.Default;
		importer.textureShape = TextureImporterShape.TextureCube;
		//importer.filterMode = FilterMode.Bilinear;
		//importer.anisoLevel = 1;
		importer.mipmapEnabled = true;
		importer.isReadable = false;
		importer.alphaIsTransparency = false;
		//importer.streamingMipmaps = true;
		importer.npotScale = TextureImporterNPOTScale.ToNearest;
		//importer.sRGBTexture = true;
		TextureImporterPlatformSettings platformSetting = importer.GetDefaultPlatformTextureSettings();
		platformSetting.format = TextureImporterFormat.Automatic;
		if (halfSize)
		{
			maxSize = Mathf.Min(maxSize, CalculateHalfSize(importer));
		}
		platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
		platformSetting.maxTextureSize = maxSize;
		platformSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
		platformSetting.textureCompression = TextureImporterCompression.Compressed;
		importer.SetPlatformTextureSettings(platformSetting);
	}

	public void PreprocessNormalMap(int maxSize, TextureImporterFormat textureFormat, bool halfSize = false)
	{
		TextureImporter importer = (TextureImporter)_processor.assetImporter;
		importer.textureType = TextureImporterType.NormalMap;
		importer.filterMode = FilterMode.Bilinear;
		importer.anisoLevel = 1;
		importer.mipmapEnabled = true;
		importer.isReadable = false;
		//importer.sRGBTexture = true;
		importer.npotScale = TextureImporterNPOTScale.ToNearest;
		TextureImporterPlatformSettings platformSetting = importer.GetDefaultPlatformTextureSettings();
		platformSetting.format = TextureImporterFormat.Automatic;
		if (halfSize)
		{
			maxSize = Mathf.Min(maxSize, CalculateHalfSize(importer));
		}
		platformSetting.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
		platformSetting.maxTextureSize = maxSize;

		platformSetting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
		platformSetting.textureCompression = TextureImporterCompression.Compressed;
		
		importer.SetPlatformTextureSettings(platformSetting);
	}

	public int CalculateHalfSize(TextureImporter importer)
	{
		int width;
		int height;
		TextureHelper.GetImageSize(importer, out width, out height);
		var halfSize = Mathf.Min(Mathf.NextPowerOfTwo(Mathf.FloorToInt((Mathf.Max(width, height) * 0.5f))), 512);
		return halfSize;
	}
}
