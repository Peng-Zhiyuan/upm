using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
[System.Serializable]
public class FogSettings
{
    public Material fogMaterial = null;
}
public class DepthHeightFogRenderFeature : ScriptableRendererFeature
{

    public FogSettings settings = new FogSettings();

    DepthHeightFogPass pass;
    public override void Create()
    {
        pass = new DepthHeightFogPass(settings, RenderPassEvent.BeforeRenderingPostProcessing);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}

public class DepthHeightFogPass : ScriptableRenderPass
{
  
    static readonly string k_RenderTag = "DepthHeightFogPass Effects";
    static readonly int TempTargetId = Shader.PropertyToID("_TempTarget");
    
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
/*
    static readonly int NoiseTexId = Shader.PropertyToID("_NoiseTex");
    static readonly int FogColorId = Shader.PropertyToID("_FogColor");

    static readonly int HeightStartId = Shader.PropertyToID("_HeightStart");
    static readonly int HeightEndId = Shader.PropertyToID("_HeightEnd");

    static readonly int DepthStartId = Shader.PropertyToID("_DepthStart");
    static readonly int DepthEndId = Shader.PropertyToID("_DepthEnd");

    static readonly int WorldPosScaleId = Shader.PropertyToID("_WorldPosScale");

    static readonly int NoiseSpXId = Shader.PropertyToID("NoiseSpX");
    static readonly int NoiseSpYId = Shader.PropertyToID("_NoiseSpY");

    static readonly int DepthNoiseScaleId = Shader.PropertyToID("_DepthNoiseScale");
    static readonly int HeightNoiseScaleId = Shader.PropertyToID("_HeightNoiseScale");

    static readonly int HeightDensityId = Shader.PropertyToID("_HeightDensity");
    static readonly int DepthDensityId = Shader.PropertyToID("_DepthDensity");

    static readonly int DepthHeightRatioId = Shader.PropertyToID("_DepthHeightRatio");
    */
    private FogSettings mFogSettings;
    DepthHeightFogVolume volume;
    Material material;
    RenderTargetIdentifier currentTarget;

    public DepthHeightFogPass(FogSettings settings,RenderPassEvent evt)
    {
        renderPassEvent = evt;
        mFogSettings = settings;
        var shader = Shader.Find("Universal Render Pipeline/CustomSSEffect/DepthHeightFog");
        if (shader == null)
        {
            Debug.LogError("Shader not found.");
            return;
        }
        material = CoreUtils.CreateEngineMaterial(shader);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogError("Material not created.");
            return;
        }

        if (!renderingData.cameraData.postProcessEnabled) return;
        /*  
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<DepthHeightFogVolume>();
        
        if (volume == null) 
        {
            Debug.LogError("volume is null");
            return; 
        }
      if (!volume.IsActive()) 
        {
            Debug.LogError("volume is not active");
            return; 
        }*/

        var cmd = CommandBufferPool.Get(k_RenderTag);
        Render(cmd, ref renderingData);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
       
    }

    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var source = currentTarget;
        int destination = TempTargetId;

        var w = cameraData.camera.scaledPixelWidth;
        var h = cameraData.camera.scaledPixelHeight;

      /*  material.SetTexture(NoiseTexId, volume.NoiseTexture.value);
        material.SetColor(FogColorId, volume.FogColor.value);
        material.SetFloat(HeightStartId, volume.HeightStart.value);
        material.SetFloat(HeightEndId, volume.HeightEnd.value);
        material.SetFloat(DepthStartId, volume.DepthStart.value);
        material.SetFloat(DepthEndId, volume.DepthEnd.value);
        material.SetFloat(WorldPosScaleId, volume.WorldPosScale.value);
        material.SetFloat(NoiseSpXId, volume.NoiseSpX.value);
        material.SetFloat(NoiseSpYId, volume.NoiseSpY.value);
        material.SetFloat(DepthNoiseScaleId, volume.DepthNoiseScale.value);
        material.SetFloat(HeightNoiseScaleId, volume.HeightNoiseScale.value);
        material.SetFloat(HeightDensityId, volume.HeightDensity.value);
        material.SetFloat(DepthDensityId, volume.DepthDensity.value);
        material.SetFloat(DepthHeightRatioId, volume.DepthHeightRatio.value);
  */
        int shaderPass = 0;
        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, mFogSettings.fogMaterial, shaderPass);
    }
}