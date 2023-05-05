using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurRenderPassFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material m_VerticalBlurMat = null;
        public Material m_HorizontalBlurMat = null;
        public string textureId = "_ScreenTexture";
        [Range(1, 8)]
        public int downSample = 1;
        [Range(1, 32)]
        public int blurCount = 1;
        [Range(0,0.005f)]
        public float intensity;
    }
    public BlurSettings blurSettings = new BlurSettings();
    RenderTargetHandle m_renderTargetHandle;
    GaussianBlurRenderPass m_gaussianBlurRenderPass;
    /// <summary>
    /// 抽象方法Create初始化
    /// </summary>
    public override void Create()
    {
        m_gaussianBlurRenderPass = new GaussianBlurRenderPass(blurSettings.renderPassEvent, blurSettings.m_VerticalBlurMat, blurSettings.m_HorizontalBlurMat, name, blurSettings.downSample, blurSettings.blurCount, blurSettings.intensity);
        m_renderTargetHandle.Init(blurSettings.textureId);
    }
    /// <summary>
    /// 抽象方法AddRenderPasses需要将使用的参数全部传入Pass
    /// </summary>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = m_renderTargetHandle;
        if (blurSettings.m_VerticalBlurMat == null)
        {
            Debug.Log("Missing VerticalBlurMat");
            return;
        }
        if (blurSettings.m_HorizontalBlurMat == null)
        {
            Debug.Log("Missing HorizontalBlurMat");
            return;
        }

        //传值
        m_gaussianBlurRenderPass.Setup(src, dest);
        renderer.EnqueuePass(m_gaussianBlurRenderPass);
    }
}