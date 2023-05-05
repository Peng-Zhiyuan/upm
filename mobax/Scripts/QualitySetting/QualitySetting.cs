using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class QualitySetting
{
    public static async void SetQualityByUIOperation(DevicePerformanceLevel q)
    {
        DeveloperLocalSettings.QualityDetected = true;
        await SetQuality(q);

    }
    public static void TryAutoSetQualityByFrame()
    {
      
        var deteced = DeveloperLocalSettings.QualityDetected;
        Debug.LogWarning("TryAutoSetQualityByFrame:"+ deteced);
        bool needDetectQuality = !deteced && DeveloperLocalSettings.GraphicQuality != DeveloperLocalSettings.Quality.Low;
        if (needDetectQuality)
        {
            QualityDetect.Stuff.Init(TrackManager.ReportDeviceInfo);
        }
    }

    public static async Task<DevicePerformanceLevel> AutoSetQualityByConf()
    {
        DevicePerformanceLevel q;
        var b = PlayerPrefs.HasKey(nameof(DeveloperLocalSettings.GraphicQuality));
        if (b)
        {
            q = (DevicePerformanceLevel)DeveloperLocalSettings.GraphicQuality;
            Debug.Log("User set quality:" + q);
        }
        else
        {
            q = DevicePerformanceUtil.GetDevicePerformanceLevel();
            Debug.Log("Auto quality:" + q);
        }

        await SetQuality(q);
        return q;
    }



    public static async Task SetQuality(DevicePerformanceLevel q)
    {
        DeveloperLocalSettings.GraphicQuality = (DeveloperLocalSettings.Quality)q;
        var address = $"soulcraft-{System.Enum.GetName(typeof(DevicePerformanceLevel),q)}Quality.asset";
        var addressableAsset = await BucketManager.Stuff.Main.GetOrAquireAsync<UniversalRenderPipelineAsset>(address);
        Debug.Log("GraphicsSettings  pipline asset address: " + address);
        GraphicsSettings.renderPipelineAsset = addressableAsset;
        RenderFeatureHandler.Ins.Clear();
        var roleRenderFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("RoleRenderFeature") as RoleRenderFeature;
        var ngSSRenderFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("NGSSRenderFeature") as NGSSRenderFeature;
        var ngSceneEffectRenderFeature = RenderFeatureHandler.Ins.GetRenderFeatureByName("NGSceneEffectRenderFeature") as NGSceneEffectRenderFeature;
      
        switch (q)
        {
            default:
            case DevicePerformanceLevel.Medium:
                {
                    if (roleRenderFeature)
                    {
                        roleRenderFeature.settings.OpenRoleOutLine = true;
                    }
                    if (ngSSRenderFeature)
                    {
                        ngSSRenderFeature.settings.OpenBloom = false;
                        ngSSRenderFeature.settings.OpenBlur = false;
                        ngSSRenderFeature.settings.OpenDistort = false;
                    }
                    if (ngSceneEffectRenderFeature)
                    {
                        ngSceneEffectRenderFeature.settings.openFog = true;
                    }
                    Shader.DisableKeyword("LEAF_RIM_LIGHT");
                    //Shader.EnableKeyword("DEPTH_TEXTURE_VALID");
                   
                    Shader.DisableKeyword("GRASS_ANIM");

                    // Shader.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    //Shader.DisableKeyword("HEIGHT_MAP");
                    //Shader.DisableKeyword("_METALLICSPECGLOSSMAP");

                    Application.targetFrameRate = 30;
                   
                   
                    QualitySettings.SetQualityLevel(2);
                    QualitySettings.renderPipeline = addressableAsset;

                    QualitySettings.masterTextureLimit = 0;
                    QualitySettings.resolutionScalingFixedDPIFactor = 1f;
                    QualitySettings.vSyncCount = 0;

                    var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
                    pipeline.renderScale = 1f;
                    break;
                }

            case DevicePerformanceLevel.Low:
                {
                    if (roleRenderFeature)
                    {
                        roleRenderFeature.settings.OpenRoleOutLine = false;
                    }
                    if (ngSSRenderFeature)
                    {
                        ngSSRenderFeature.settings.OpenBloom = false;
                        ngSSRenderFeature.settings.OpenBlur = false;
                        ngSSRenderFeature.settings.OpenDistort = false;
                    }
                    if (ngSceneEffectRenderFeature)
                    {
                        ngSceneEffectRenderFeature.settings.openFog = false;
                    }
                    Shader.DisableKeyword("LEAF_RIM_LIGHT");
                    //Shader.EnableKeyword("DEPTH_TEXTURE_VALID");
                   
                    Shader.DisableKeyword("GRASS_ANIM");

                   // Shader.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");

                    //Shader.DisableKeyword("HEIGHT_MAP");
                    //Shader.DisableKeyword("_METALLICSPECGLOSSMAP");

                    Application.targetFrameRate = 30;
            
                    QualitySettings.SetQualityLevel(1);
                    QualitySettings.renderPipeline = addressableAsset;

                    QualitySettings.masterTextureLimit = 1;
                    QualitySettings.resolutionScalingFixedDPIFactor = 1f;
                    QualitySettings.vSyncCount = 0;
                    //QualitySettings.skinWeights = SkinWeights.TwoBones;

                    var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
                    pipeline.renderScale = 0.8f;
                    break;
                }
            case DevicePerformanceLevel.Hight:
                {
                    if (roleRenderFeature)
                    {
                        roleRenderFeature.settings.OpenRoleOutLine = true;
                    }
                    if (ngSSRenderFeature)
                    {
                        ngSSRenderFeature.settings.OpenBloom = true;
                        ngSSRenderFeature.settings.OpenBlur = true;
                        ngSSRenderFeature.settings.OpenDistort = true;
                    }
                    if (ngSceneEffectRenderFeature)
                    {
                        ngSceneEffectRenderFeature.settings.openFog = true;
                    }
                    Shader.EnableKeyword("LEAF_RIM_LIGHT");
                    Shader.EnableKeyword("GRASS_ANIM");
                    //Shader.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    //Shader.EnableKeyword("HEIGHT_MAP");
                    
                    Application.targetFrameRate = 60;
                    QualitySettings.SetQualityLevel(3);
                    QualitySettings.renderPipeline = addressableAsset;

                    QualitySettings.masterTextureLimit = 0;
                    QualitySettings.resolutionScalingFixedDPIFactor = 1f;
                    QualitySettings.vSyncCount = 0;

                    var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
                    pipeline.renderScale = 1f;
                    break;
                }
        }
        Battle.Instance.TryRefreshBattleRender();
        //DynamicGI.UpdateEnvironment();

    }


    
}
