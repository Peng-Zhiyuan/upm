using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;
using UnityEngine.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Preserve]
public static class PerformanceCommand
{
    public static async void SetQualityLow()
    {
        await QualitySetting.SetQuality(DevicePerformanceLevel.Low);
    }

    public static async void SetQualityMedium()
    {
        await QualitySetting.SetQuality(DevicePerformanceLevel.Medium);
    }

    public static async void SetQualityHeight()
    {
        await QualitySetting.SetQuality(DevicePerformanceLevel.Hight);
    }

   
/*    public static void SetQualityLevel(int q)
    {
        QualitySettings.SetQualityLevel(q);
    }*/

    public static void SetMasterTextureLimit(int limit)
    {
        QualitySettings.masterTextureLimit = limit;
    }
    public static void SetSyncCount(int syncCount)
    {
        QualitySettings.vSyncCount = syncCount;
    }

    public static void SetDpiFactor(int dpi)
    {
        QualitySettings.resolutionScalingFixedDPIFactor = dpi/100f;
    }

    public static  void SetFrameRate(int frame)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frame;
    }

    public static void RenderScale(int scale)
    {
        var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
        pipeline.renderScale = scale / 100f;
    }

    public static void OpenHeightMap()
    {
        Shader.EnableKeyword("HEIGHT_MAP");
    }

    public static void CloseHeightMap()
    {
        Shader.DisableKeyword("HEIGHT_MAP");
    }

    /*   public static void SetShadowDistance(int distance)
        {
            var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
            pipeline.shadowDistance = distance;
        }*/

    /*    public static void SetMSAA(int msaaSample)
        {
            var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
            pipeline.msaaSampleCount = msaaSample;
        }

        public static void SetShadowCascadeCount(int cascadeCount)
        {
            var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
            pipeline.shadowCascadeCount = cascadeCount;
        }*/

    /*    public static void SetShadowCascadeCount(int cascadeCount)
        {
            var pipeline = ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
            pipeline.shadowCascadeCount = cascadeCount;
        }*/
}