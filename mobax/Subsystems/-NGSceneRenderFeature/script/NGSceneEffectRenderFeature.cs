using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
[System.Serializable]
public class NGSceneEffectSettings
{
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    public bool openFog = false;
    public Material lutMaterial = null;
    public Material fadeMaterial = null;
    public Material colorAdjustMaterial = null;
    public Material fogMaterial = null;
    public Material radiaBlurMaterial = null;
}
public class NGSceneEffectRenderFeature : ScriptableRendererFeature
{

    public NGSceneEffectSettings settings = new NGSceneEffectSettings();

    NGSceneEffectPass pass;

    public override void Create()
    {
        pass = new NGSceneEffectPass(settings, settings.renderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}

public class NGSceneEffectPass : ScriptableRenderPass
{
  
    static readonly string k_RenderTag = "NGSceneEffectPass";
    private NGSSRenderSettings ngSSRenderSettings;
    //static readonly int TempTargetId = Shader.PropertyToID("_TempTarget");

    //static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    public NGSceneEffectSettings mEffSettings;
    private NGRenderBuffer ngRenderBuffer;
    string mProfilerTag = "SceneEffectRender";
    //SSEffectVolume volume;
    //public Material material;
    //RenderTargetIdentifier currentTarget;
    //Profiling上显示
    ProfilingSampler m_EnvLutSampler = new ProfilingSampler("EnvLUT");
    //辅助RT

    ProfilingSampler m_FadeBlackSampler = new ProfilingSampler("FadeBlack");
    ProfilingSampler m_ColorAdjustSampler = new ProfilingSampler("ColorAdjust");
    int _ColorAdjustmentsId = Shader.PropertyToID("_ColorAdjustments");
    int _ColorFilterId = Shader.PropertyToID("_ColorFilter");
    int _LUTParametersId = Shader.PropertyToID("_LUTParameters");
    int _StrengthId = Shader.PropertyToID("_Strength");
    int _LUTId = Shader.PropertyToID("_LUT");
    int _CenterXId = Shader.PropertyToID("_CenterX");
    int _CenterYId = Shader.PropertyToID("_CenterY");
    private FilteringSettings mDecalFilter;
    private ShaderTagId mDecalShaderTag;
    public NGSceneEffectPass(NGSceneEffectSettings settings,RenderPassEvent evt)
    {
        renderPassEvent = evt;
        mEffSettings = settings;
        ngRenderBuffer = new NGRenderBuffer();

        mDecalShaderTag = new ShaderTagId("GroundDecal");
        RenderQueueRange queue = new RenderQueueRange();
        queue.lowerBound = 2000;
        queue.upperBound = 3000;
        //int decalLayer = 1 << LayerMask.NameToLayer("GroundDecal") | 1 << LayerMask.NameToLayer("Default");
        mDecalFilter = new FilteringSettings(queue);
    }
    /*
    private Dictionary<Camera, CameraCustomData> cacheCameraData = new Dictionary<Camera, CameraCustomData>();
    private CameraCustomData GetCustomData(Camera camera)
    {
  
        CameraCustomData data;
        if (cacheCameraData.TryGetValue(camera, out data))
        {
            if (data != null) return data;
        }
        CameraCustomData customData = camera.transform.GetComponent<CameraCustomData>();
        cacheCameraData[camera] = customData;
        return customData;
    }
  */


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        DrawingSettings decalDrawSetting = CreateDrawingSettings(mDecalShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
        context.DrawRenderers(renderingData.cullResults, ref decalDrawSetting, ref mDecalFilter);
        CameraCustomData customData = RenderFeatureCache.GetCustomData(renderingData.cameraData.camera);
        CommandBuffer cmd = CommandBufferPool.Get(mProfilerTag);
        ngRenderBuffer.InitCustomSource(renderingData);

        if (customData != null && mEffSettings.radiaBlurMaterial != null && customData.RadiaBlur.openSceneBlur)
        {
            // ngSSRenderSettings.radiaBlurMaterial.SetFloat("_Level", customData.RadiaBlur.radiaBlurLevel);
            mEffSettings.radiaBlurMaterial.SetFloat(_CenterXId, customData.RadiaBlur.radiaBlurCenterX);
            mEffSettings.radiaBlurMaterial.SetFloat(_CenterYId, customData.RadiaBlur.radiaBlurCenterY);
            // ngSSRenderSettings.radiaBlurMaterial.SetFloat("_BufferRadius", customData.RadiaBlur.radiaBlurBufferRadius);

            cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mEffSettings.radiaBlurMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            ngRenderBuffer.SwapBuffer();
        }


        if (mEffSettings.openFog && customData != null && mEffSettings.fogMaterial != null && customData.DepthHeightFog.openFog)
        {
            cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mEffSettings.fogMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            ngRenderBuffer.SwapBuffer();
        }
        
        if (customData != null && mEffSettings.lutMaterial != null && customData.EnvLUT.Open)
        {
            //Debug.LogError("m_EnvLutSampler");

            //using的做法就是可以在FrameDebug上看到里面的所有渲染
            using (new ProfilingScope(cmd, m_EnvLutSampler))
            {
                //创建一张RT
                // RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                // opaqueDesc.depthBufferBits = 0;
                // cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, FilterMode.Bilinear);
                int lut_height = 32;
                int lut_width = 32 * 32;
                mEffSettings.lutMaterial.SetVector(_LUTParametersId, new Vector3(1f / lut_width, 1f / lut_height, lut_height - 1f));
                //mEffSettings.lutMaterial.SetVector("_LUTParameters", new Vector4(lut_height, 0.5f/ lut_width, 0.5f/lut_height, lut_height /(lut_height - 1f)));
                mEffSettings.lutMaterial.SetFloat(_StrengthId, customData.EnvLUT.Strength);
                mEffSettings.lutMaterial.SetTexture(_LUTId, customData.EnvLUT.LUTTexture);
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mEffSettings.lutMaterial);
            }
            //执行
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            ngRenderBuffer.SwapBuffer();
        }

        if (customData != null && customData.colorAdjustments.openColorAdjustments && mEffSettings.colorAdjustMaterial != null)
        {
            //Debug.LogError("openColorAdjustments");
            using (new ProfilingScope(cmd, m_ColorAdjustSampler))
            {

                customData.colorAdjustments.ColorFilter.a = 1 - customData.colorAdjustments.fadeScene;
                mEffSettings.colorAdjustMaterial.SetVector(this._ColorAdjustmentsId, new Vector4(Mathf.Pow(2f, customData.colorAdjustments.PostExposure),
                customData.colorAdjustments.Contrast * 0.01f + 1f,
                customData.colorAdjustments.HueShift * (1.0f / 360f),
                customData.colorAdjustments.Saturation * 0.01f + 1f));
                mEffSettings.colorAdjustMaterial.SetColor(this._ColorFilterId, customData.colorAdjustments.ColorFilter.linear);
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), mEffSettings.colorAdjustMaterial, 0);
            }
            //执行
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            ngRenderBuffer.SwapBuffer();
        }
        ngRenderBuffer.FinalCopy(context, cmd);
        CommandBufferPool.Release(cmd);

      
    }

    public void Setup(in RenderTargetIdentifier source)
    {

        ngRenderBuffer.Setup(source);

    }

}