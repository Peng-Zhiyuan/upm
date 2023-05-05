using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[System.Serializable]
public class NGSSRenderSettings
{
    public bool OpenBloom = true;
    public bool OpenBlur = true;
    public bool OpenDistort = true;
    public Material radiaBlurMaterial = null;

    public Material blackWhiteFlashMaterial = null;

    public Material bloomMaterial = null;
    [Range(2, 6)] public int MaxBloomIterations = 6;

    //public Material fogMaterial = null;

    public Material cloudShadowMaterial = null;
    /*
    [Tooltip("叠加颜色")]
    public Color m_Color = Color.white;
    [Tooltip("XY为起始位置，zw为XY移动速度")]
    public Vector4 m_StartXYSpeedXY = new Vector4(5, 5, 0, 0);
    [Tooltip("贴图缩放大小，值越小越大")]
    public float m_Scale = 0.1f;
    [Tooltip("可以移动的大小范围")]
    public float m_WorldSize = 200;
    */


}
public class NGSSRenderFeature : ScriptableRendererFeature
{
  

    public class NGSSRenderPass : ScriptableRenderPass
    {
        private NGSSRenderSettings ngSSRenderSettings;
        private string mProfilerTag;
        private RenderTextureDescriptor rtDescBase;
        private int mIterations;
        private int mRange;
        private RenderTexture[] rtTexture;
        private RenderTargetIdentifier[] rt;
        private RenderTexture rt2Texture;
        private RenderTargetIdentifier rt2;
       

        private NGRenderBuffer ngRenderBuffer;
        private NGGrabTexturePass ngGrabTexturePass;
        private CloudShadowPass ngCloudShadowPass;
        private ShaderTagId mDistortShaderTag;
        int _FlashId = Shader.PropertyToID("_Flash");
        int _CenterXId = Shader.PropertyToID("_CenterX");
        int _CenterYId = Shader.PropertyToID("_CenterY");
        int _BloomId = Shader.PropertyToID("_Bloom");
        int _BlurScaleId = Shader.PropertyToID("_BlurScale");
        int _BloomTexId = Shader.PropertyToID("_BloomTex");
        
        public NGSSRenderPass(RenderPassEvent renderPassEvent, NGSSRenderSettings renderSettings, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            ngSSRenderSettings = renderSettings;
            mProfilerTag = tag;
            ngRenderBuffer = new NGRenderBuffer();
            ngGrabTexturePass = new NGGrabTexturePass();
            if (ngSSRenderSettings.cloudShadowMaterial != null)
            {
                /*
                ngSSRenderSettings.cloudShadowMaterial.SetColor("_Color", ngSSRenderSettings.m_Color);
                //ngSSRenderSettings.SetTexture("_CloudTex", ngSSRenderSettings.m_Tex);
                ngSSRenderSettings.cloudShadowMaterial.SetVector("_StartXYSpeedXY", ngSSRenderSettings.m_StartXYSpeedXY);
                ngSSRenderSettings.cloudShadowMaterial.SetFloat("_Scale", ngSSRenderSettings.m_Scale);
                ngSSRenderSettings.cloudShadowMaterial.SetFloat("_WorldSize", ngSSRenderSettings.m_WorldSize);
                */
                ngCloudShadowPass = new CloudShadowPass(ngSSRenderSettings.cloudShadowMaterial);
            }

            mDistortShaderTag = new ShaderTagId("Distort");
            RenderQueueRange queue = new RenderQueueRange();
            queue.lowerBound = 1000;
            queue.upperBound = 4000;
            //int decalLayer = 1 << LayerMask.NameToLayer("GroundDecal") | 1 << LayerMask.NameToLayer("Default");
            mDistortFilter = new FilteringSettings(queue);
        }

        public void Setup(RenderTargetIdentifier source)
        {
            ngRenderBuffer.Setup(source);
            ngGrabTexturePass.Setup(ngRenderBuffer);
            if(ngCloudShadowPass != null) ngCloudShadowPass.Setup(ngRenderBuffer);
        }

        private FilteringSettings mDistortFilter;
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //CameraCustomData customData = renderingData.cameraData.camera.transform.GetComponent<CameraCustomData>();
            CameraCustomData customData = RenderFeatureCache.GetCustomData(renderingData.cameraData.camera);
            if (customData == null || !customData.enabled)
            {
                return;
            }

            ngRenderBuffer.InitCustomSource(renderingData);

          



            CommandBuffer cmd = CommandBufferPool.Get(mProfilerTag);
            /*
            if (ngSSRenderSettings.greyWhiteMaterial != null && customData.GreyWhite.openGreyWhite)
            {
                ngSSRenderSettings.greyWhiteMaterial.SetFloat("_Amount", customData.GreyWhite.greyWhiteAmount);
                ngSSRenderSettings.greyWhiteMaterial.SetInt("_Switch", customData.GreyWhite.greyWhiteSwitch);
                ngSSRenderSettings.greyWhiteMaterial.SetInt("_Change", customData.GreyWhite.greyWhiteChange);
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.greyWhiteMaterial);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }
            */
          
            if (ngSSRenderSettings.blackWhiteFlashMaterial != null && customData.BlackWhiteFlash.openBlackWhiteFlash)
            {
                ngSSRenderSettings.blackWhiteFlashMaterial.SetFloat(_FlashId, customData.BlackWhiteFlash.flash?1:0);
                ngSSRenderSettings.blackWhiteFlashMaterial.SetFloat(_CenterXId, customData.BlackWhiteFlash.flashCenterX);
                ngSSRenderSettings.blackWhiteFlashMaterial.SetFloat(_CenterYId, customData.BlackWhiteFlash.flashCenterY);
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.blackWhiteFlashMaterial);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }
          
            if (ngSSRenderSettings.OpenBlur && ngSSRenderSettings.radiaBlurMaterial != null && customData.RadiaBlur.openBlur)
            { 
               // ngSSRenderSettings.radiaBlurMaterial.SetFloat("_Level", customData.RadiaBlur.radiaBlurLevel);
                ngSSRenderSettings.radiaBlurMaterial.SetFloat(_CenterXId, customData.RadiaBlur.radiaBlurCenterX);
                ngSSRenderSettings.radiaBlurMaterial.SetFloat(_CenterYId, customData.RadiaBlur.radiaBlurCenterY);
               // ngSSRenderSettings.radiaBlurMaterial.SetFloat("_BufferRadius", customData.RadiaBlur.radiaBlurBufferRadius);

                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.radiaBlurMaterial);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }

           

            bool openFlash = customData.BlackWhiteFlash.openBlackWhiteFlash;
            /*
            if (ngSSRenderSettings.fogMaterial != null && customData.DepthHeightFog.openFog && !openFlash)
            {
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.fogMaterial);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                ngRenderBuffer.SwapBuffer();
            }
            */
            

            if (ngSSRenderSettings.cloudShadowMaterial != null && customData.CloudShadow.openCloudShadow && !openFlash)
            {
                /*
                ngSSRenderSettings.cloudShadowMaterial.SetColor("_Color", ngSSRenderSettings.m_Color);
                //mat.SetTexture("_CloudTex", ngSSRenderSettings.m_Tex);
                ngSSRenderSettings.cloudShadowMaterial.SetVector("_StartXYSpeedXY", ngSSRenderSettings.m_StartXYSpeedXY);
                ngSSRenderSettings.cloudShadowMaterial.SetFloat("_Scale", ngSSRenderSettings.m_Scale);
                ngSSRenderSettings.cloudShadowMaterial.SetFloat("_WorldSize", ngSSRenderSettings.m_WorldSize);
                m_Pass = new CloudShadowPass(ngSSRenderSettings.cloudShadowMaterial, renderPassEvent);
                */
               // ngCloudShadowPass.Execute(cmd, context, ref renderingData);

                //cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.cloudShadowMaterial);
                //context.ExecuteCommandBuffer(cmd);
                //cmd.Clear();
                //ngRenderBuffer.SwapBuffer();
            }

            if (ngSSRenderSettings.OpenBloom && ngSSRenderSettings.bloomMaterial != null  && customData.Bloom.openBloom && !openFlash)
            {
                if(customData.Bloom.bloomQuality == CameraCustomData.BloomQualityEnum.HighQuality)
                {
                    mIterations = ngSSRenderSettings.MaxBloomIterations;
                }
                else
                {
                    mIterations = ngSSRenderSettings.MaxBloomIterations - 3;
                }
                //if (customData.Bloom.bloomState == CameraCustomData.BloomSettingEnum.Custom)
                //{
                    mRange = Mathf.Max(0, customData.Bloom.Range - (6 - ngSSRenderSettings.MaxBloomIterations));
                    if (ngSSRenderSettings.bloomMaterial != null)
                    {
                        float threshold = customData.Bloom.BloomThreshold;
                        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                        {
                            threshold *= threshold;
                        }
                        ngSSRenderSettings.bloomMaterial.SetVector(_BloomId, new Vector4(customData.Bloom.BloomIntensity, 0, 0, threshold));
                    }
                //}
                /*
                else
                {
                    mRange = Mathf.Max(0, ngSSRenderSettings.Range - (6 - ngSSRenderSettings.MaxIterations));
                    if (ngSSRenderSettings.bloomMaterial != null)
                    {
                        float threshold = ngSSRenderSettings.BloomThreshold;
                        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                        {
                            threshold *= threshold;
                        }
                        ngSSRenderSettings.bloomMaterial.SetVector("_Bloom", new Vector4(ngSSRenderSettings.BloomIntensity, 0, 0, threshold));
                    }
                }
                */

               // rtFormat = renderingData.cameraData.camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
               // rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float) ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.ARGB32;
                rtDescBase = renderingData.cameraData.cameraTargetDescriptor;
                rtDescBase.msaaSamples = 1;
              //  rtDescBase.colorFormat = rtFormat;
                rtDescBase.depthBufferBits = 0;
                int width;
                int height;
                float aspectRatio = (float)rtDescBase.height / rtDescBase.width;
                if (aspectRatio < 1)
                {
                    width = (int)Mathf.Pow(2, mIterations + 4);
                    while (width >= rtDescBase.width)
                    {
                        mIterations--;
                        width = (int)Mathf.Pow(2, mIterations + 4);
                    }
                    height = Mathf.Max(1, (int)(width * aspectRatio));
                }
                else
                {
                    height = (int)Mathf.Pow(2, mIterations + 4);
                    while (height >= rtDescBase.height)
                    {
                        mIterations--;
                        height = (int)Mathf.Pow(2, mIterations + 4);
                    }
                    width = Mathf.Max(1, (int)(height / aspectRatio));
                }

                if (mRange < 0)
                {
                    return;
                }

                // Bloom buffers
                if (rt == null || rt.Length != mRange + 1)
                {
                    rt = new RenderTargetIdentifier[mRange + 1];
                    rtTexture = new RenderTexture[mRange + 1];
                }
                RenderTextureDescriptor rtBloomDescriptor = rtDescBase;
                rtBloomDescriptor.width = width;
                rtBloomDescriptor.height = height;
                rtTexture[0] = RenderTexture.GetTemporary(rtBloomDescriptor);
                rt[0] = rtTexture[0];
                rt2Texture = RenderTexture.GetTemporary(rtBloomDescriptor);
                rt2 = rt2Texture;
                cmd.Blit(ngRenderBuffer.GetSource(), rt2, ngSSRenderSettings.bloomMaterial, 1);

                cmd.SetGlobalFloat(_BlurScaleId, 1);
                cmd.Blit(rt2, rt[0], ngSSRenderSettings.bloomMaterial, 2);

                context.ExecuteCommandBuffer(cmd);

                cmd.Clear();

                if (rt2Texture != null)
                {
                    RenderTexture.ReleaseTemporary(rt2Texture);
                }

                width >>= 1;
                height >>= 1;
                for (int i = 1; i <= mRange; i++)
                {
                    rtBloomDescriptor.width = Mathf.Max(width,1);
                    rtBloomDescriptor.height = Mathf.Max(height,1);
                    rtTexture[i] = RenderTexture.GetTemporary(rtBloomDescriptor);
                    rt[i] = rtTexture[i];
                    width >>= 1;
                    height >>= 1;
                }

                for (int i = 0; i < mRange; i++)
                {
                    cmd.SetGlobalFloat(_BlurScaleId, 2);
                    cmd.Blit(rt[i], rt[i + 1], ngSSRenderSettings.bloomMaterial, 2);
                }

                context.ExecuteCommandBuffer(cmd);

                cmd.Clear();

                for (int i = mRange; i > 0; i--)
                {
                    rtTexture[i - 1].MarkRestoreExpected();
                    rt[i - 1] = rtTexture[i - 1];
                    cmd.Blit(rt[i], rt[i - 1], ngSSRenderSettings.bloomMaterial, 3);
                }
                cmd.SetGlobalTexture(_BloomTexId, rt[0]);
                cmd.Blit(ngRenderBuffer.GetSource(), ngRenderBuffer.GetTarget(), ngSSRenderSettings.bloomMaterial, 0);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                if (rtTexture != null)
                {
                    for (int i = 0; i < rtTexture.Length; i++)
                    {
                        if (rtTexture[i] != null)
                        {
                            RenderTexture.ReleaseTemporary(rtTexture[i]);
                            rtTexture[i] = null;
                        }
                    }
                }
                ngRenderBuffer.SwapBuffer();
            }
            if (ngSSRenderSettings.OpenDistort)
            {
                cmd.SetGlobalTexture("_GrabTexture", ngRenderBuffer.GetSource());
            }
           
            //ngGrabTexturePass.Execute(this, context, ref renderingData);
            ngRenderBuffer.FinalCopy(context, cmd);
         
            CommandBufferPool.Release(cmd);

            if (ngSSRenderSettings.OpenDistort)
            {
                DrawingSettings distortDrawSetting = CreateDrawingSettings(mDistortShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
                context.DrawRenderers(renderingData.cullResults, ref distortDrawSetting, ref mDistortFilter);
            }
        }
    }

    public NGSSRenderSettings settings = new NGSSRenderSettings();

    NGSSRenderPass ssRenderPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        ssRenderPass.Setup(renderer.cameraColorTarget);

        renderer.EnqueuePass(ssRenderPass);
    }

    public override void Create()
    {
       /* FuntoyBloomSettings.BloomThreshold = settings.BloomThreshold;
        FuntoyBloomSettings.Range = settings.Range;
        FuntoyBloomSettings.BloomIntensity = settings.BloomIntensity;
        FuntoyBloomSettings.MaxIterations = settings.MaxIterations;*/
        ssRenderPass = new NGSSRenderPass(RenderPassEvent.BeforeRenderingPostProcessing, settings, name);
    }
}
