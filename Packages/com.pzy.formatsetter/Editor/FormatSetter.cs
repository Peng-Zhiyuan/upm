using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class FormatSetter : AssetPostprocessor
{

    private const string PATH2 = "pzy.com.*/FormatSetter/Auto Set Fromat";


    [MenuItem(PATH2)]
    public static void ToggleAutoFormat()
    {
        var b = FormatSetter.autoChnageSettingsWhenImport;
        var newValue = !b;
        FormatSetter.autoChnageSettingsWhenImport = newValue;
    }

    [MenuItem(PATH2, true)]
    public static bool ToggleAutoFormatValidate()
    {
        var isChecked = FormatSetter.autoChnageSettingsWhenImport;
        Menu.SetChecked(PATH2, isChecked);
        return true;
    }

    static FormatSetterSettings _settings;
    public static FormatSetterSettings Settings
    {
        get
        {
            if(_settings == null)
            {
                var guidList = AssetDatabase.FindAssets($"t: {nameof(FormatSetterSettings)}");
                if(guidList.Length == 0)
                {
                    //throw new Exception("[FormatSetter] no FormatSetterSettings found");
                    return null;
                }
                var guid = guidList[0];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<FormatSetterSettings>(path);
                if(asset == null)
                {
                    throw new Exception($"[FormatSetter] {path} is not a FormatSetterSettings");
                }
                _settings = asset;
            }
            return _settings;
        }
    }

    static bool HasSettingsAndEnableProcessTexture
    {
        get
        {
            var setting = Settings;
            if(setting == null)
            {
                return false;
            }
            var isEnable = setting.enableProcessTexture;
            if(!isEnable)
            {
                return false;
            }
            return true;
        }
    }

    static bool HasSettingsAndEnableProcessAudioClip
    {
        get
        {
            var setting = Settings;
            if (setting == null)
            {
                return false;
            }
            var isEnable = setting.enableProcessAudioClip;
            if (!isEnable)
            {
                return false;
            }
            return true;
        }
    }

    public static TextureImporterFormat GetTextureFormateBySettings(string path)
    {
        var settings = Settings;

        if(settings.filterList != null)
        {
            var filterList = settings.filterList;
            foreach (var filter in filterList)
            {
                var keyWorkd = filter.keyWord;
                var isMatched = path.Contains(keyWorkd);
                if (isMatched)
                {
                    return filter.format;
                }
            }
        }

        var defaultFormat = settings.defaultFormat;
        return defaultFormat;
    }

    [MenuItem("pzy.com.*/FormatSetter/Reset All Importer")]
    public static void ResetAllImporter()
    {
        ResetAndReimport();
    }

    static void ResetAndReimport(bool testTen = false)
    {

        autoChnageSettingsWhenImport = false;

        try
        {
            var allAssetPathList = AssetDatabase.GetAllAssetPaths();

            var chnagedImporterList = new List<AssetImporter>();
            foreach (var path in allAssetPathList)
            {
                // 不检查 Packages 中的资产
                if (path.StartsWith("Packages/"))
                {
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path);
                var changed = TryResetImporter(path, importer);
                if (changed)
                {
                    chnagedImporterList.Add(importer);
                }

                if (testTen)
                {
                    var nowCount = chnagedImporterList.Count;
                    if (nowCount >= 10)
                    {
                        break;
                    }
                }
            }

            var count = chnagedImporterList.Count;
            if (count == 0)
            {
                EditorUtility.DisplayDialog("", $"没有需要导入的资源", "ok");
                return;
            }

            var b = EditorUtility.DisplayDialog("", $"为 {count} 个资源设置了格式，需要重新导入吗?", "ok", "cancel");

            if (b)
            {
                ReimportList(chnagedImporterList);
                Debug.Log($"{count} importer chnage and reimported");
            }
        }
        finally
        {
            autoChnageSettingsWhenImport = true;
        }

    }

    [MenuItem("pzy.com.*/FormatSetter/Test Reset 10 Importer")]
    public static void TestReset10Importer()
    {
        ResetAndReimport(true);
    }

    static void ReimportList(List<AssetImporter> importerList)
    {
        if(importerList.Count == 0)
        {
            return;
        }

        var count = importerList.Count;
        var proccessedCount = 0;
        EditorUtility.DisplayCancelableProgressBar("Reimporter", $"{proccessedCount}/{count}", (float)proccessedCount/count);
        var lastShowTime = DateTime.Now;
        foreach (var importer in importerList)
        {
            importer.SaveAndReimport();
            proccessedCount++;
            var now = DateTime.Now;
            var delta = now - lastShowTime;
            var ms = delta.TotalMilliseconds;
            if(ms >= 500)
            {
                lastShowTime = now;
                var canceld = EditorUtility.DisplayCancelableProgressBar("Reimporter", $"{proccessedCount}/{count}", (float)proccessedCount / count);
                if(canceld)
                {
                    EditorUtility.ClearProgressBar();
                    throw new Exception("canceled");
                }
            }
        }

        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 将 import 设置成理想值
    /// </summary>
    /// <param name="importer">修改并重新导入过的资源</param>
    /// <returns></returns>
    static bool TryResetImporter(string path, AssetImporter importer)
    {
        if (importer is TextureImporter)
        {
            var changed = ResetTextureImporter(path, (TextureImporter)importer);
            //if(changed)
            //{
            //    Debug.Log($"texture: {path}");
            //}
            return changed;
        }
        else if (importer is AudioImporter)
        {
            var changed = ResetAudioImporter((AudioImporter)importer);
            //if(changed)
            //{
            //    Debug.Log($"audio: {path}");
            //}
            return changed;
            
        }
        else
        {
            // not care importer type
            return false;
        }
    }

    static bool ResetTextureImporter(string path, TextureImporter textureImporter)
    {
        if(!HasSettingsAndEnableProcessTexture)
        {
            return false;
        }

        var changed = false;
        var targetFormate = GetTextureFormateBySettings(path);

        // 禁用 mipmap
        if (textureImporter.mipmapEnabled != false)
        {
            textureImporter.mipmapEnabled = false;
            changed = true;
        }

        // ios
        {
            var platformSettings = textureImporter.GetPlatformTextureSettings("iPhone");

            if (platformSettings == null)
            {
                platformSettings = new TextureImporterPlatformSettings();
                platformSettings.name = "iPhone";
                changed = true;
            }

            if (platformSettings.overridden != true)
            {
                platformSettings.overridden = true;
                changed = true;
            }

            
            if (platformSettings.format != targetFormate)
            {
                platformSettings.format = targetFormate;
                changed = true;
            }

            textureImporter.SetPlatformTextureSettings(platformSettings);
        }

        // android
        {
            var platformSettings = textureImporter.GetPlatformTextureSettings("android");

            if (platformSettings == null)
            {
                platformSettings = new TextureImporterPlatformSettings();
                platformSettings.name = "android";
                changed = true;
            }

            if (platformSettings.overridden != true)
            {
                platformSettings.overridden = true;
                changed = true;
            }

            if (platformSettings.format != targetFormate)
            {
                platformSettings.format = targetFormate;
                changed = true;
            }

            textureImporter.SetPlatformTextureSettings(platformSettings);
        }


        //// ios
        //{
        //    var platformSettings = textureImporter.GetPlatformTextureSettings("iPhone");

        //    if(platformSettings == null)
        //    {
        //        platformSettings = new TextureImporterPlatformSettings();
        //        platformSettings.name = "iPhone";
        //        changed = true;
        //    }

        //    if(platformSettings.overridden != true)
        //    {
        //        platformSettings.overridden = true;
        //        changed = true;
        //    }

        //    if(platformSettings.format != TextureImporterFormat.ASTC_RGBA_4x4)
        //    {
        //        platformSettings.format = TextureImporterFormat.ASTC_RGBA_4x4;
        //        changed = true;
        //    }

        //    textureImporter.SetPlatformTextureSettings(platformSettings);
        //}

        //// android
        //{
        //    var platformSettings = textureImporter.GetPlatformTextureSettings("android");

        //    if (platformSettings == null)
        //    {
        //        platformSettings = new TextureImporterPlatformSettings();
        //        platformSettings.name = "android";
        //        changed = true;
        //    }

        //    if (platformSettings.overridden != true)
        //    {
        //        platformSettings.overridden = true;
        //        changed = true;
        //    }

        //    if (platformSettings.format != TextureImporterFormat.ETC2_RGBA8)
        //    {
        //        platformSettings.format = TextureImporterFormat.ETC2_RGBA8;
        //        changed = true;
        //    }

        //    textureImporter.SetPlatformTextureSettings(platformSettings);
        //}
        return changed;
    }


    static bool ResetAudioImporter(AudioImporter audioImporter)
    {
        if (!HasSettingsAndEnableProcessAudioClip)
        {
            return false;
        }

        var changed = false;

        // 关闭双声道
        if(audioImporter.forceToMono != false)
        {
            audioImporter.forceToMono = false;
            changed = true;
        }
        

        // 关闭环绕音
        if(audioImporter.ambisonic != false)
        {
            audioImporter.ambisonic = false;
            changed = true;
        }
        

        // 在加载 asset 时加载数据
        if(audioImporter.preloadAudioData != true)
        {
            audioImporter.preloadAudioData = true;
            changed = true;
        }
        

        // 在后台加载数据
        if(audioImporter.loadInBackground != true)
        {
            audioImporter.loadInBackground = true;
            changed = true;
        }

        {
            var beforeSetting = audioImporter.defaultSampleSettings;

            var settings = new AudioImporterSampleSettings();

            // 在加载数据（不是 asset）时，解压
            settings.loadType = AudioClipLoadType.DecompressOnLoad;

            // 使用 Vorbis 压缩格式
            settings.compressionFormat = AudioCompressionFormat.Vorbis;

            // Vorbis 压缩质量
            settings.quality = 0.5f;

            // 优化采样率
            settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;

            audioImporter.defaultSampleSettings = settings;

            if(!beforeSetting.Equals(settings))
            {
                changed = true;
            }
        }

        return changed;
    }

    public void OnPreprocessAudio()
    {
        if (!HasSettingsAndEnableProcessAudioClip)
        {
            return;
        }

        if (autoChnageSettingsWhenImport)
        {
            var path = assetPath;

            // 不检查 Packages 中的资产
            if (path.StartsWith("Packages/"))
            {
                return;
            }

            Debug.Log($"auto change audio settings: {assetPath}");
            var audioImporter = (AudioImporter)assetImporter;
            ResetAudioImporter(audioImporter);
        }
    }


    public static bool autoChnageSettingsWhenImport = true;
    void OnPreprocessTexture()
    {
        if (!HasSettingsAndEnableProcessTexture)
        {
            return;
        }

        if (autoChnageSettingsWhenImport)
        {
            var path = assetPath;

            // 不检查 Packages 中的资产
            if (path.StartsWith("Packages/"))
            {
                return;
            }

            Debug.Log($"auto change texture settings: {assetPath}");
            var textureImporter = (TextureImporter)assetImporter;
            ResetTextureImporter(assetPath, textureImporter);
        }
    }

    


}
