using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurRenderPass : ScriptableRenderPass
{
    public Material m_VerticalBlurMat;
    public Material m_HorizontalBlurMat;
    public FilterMode filterMode { get; set; }
    private int blurCount;
    private int downSample;
    private float intensity;
    private RenderTargetIdentifier source { get; set; }
    private RenderTargetHandle destination { get; set; }

    //第一次处理结果
    RenderTargetHandle m_temporaryColorTexture01;
    //第二次处理结果
    RenderTargetHandle m_temporaryColorTexture02;
    //最后的结果
    RenderTargetHandle m_temporaryColorTexture03;

    string m_ProfilerTag;
    public GaussianBlurRenderPass(RenderPassEvent renderPassEvent, Material VerticalBlurMat, Material HorizontalBlurMat, string tag, int downSample, int blurCount, float intensity)
    {
        this.renderPassEvent = renderPassEvent;
        m_VerticalBlurMat = VerticalBlurMat;
        m_HorizontalBlurMat = HorizontalBlurMat;
        this.downSample = downSample;
        this.blurCount = blurCount;
        this.intensity = intensity;
        m_ProfilerTag = tag;
        m_temporaryColorTexture01.Init("_temporaryColorTexture1");
        m_temporaryColorTexture02.Init("_temporaryColorTexture2");
        m_temporaryColorTexture03.Init("_temporaryColorTexture3");
    }
    public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
    {
        this.source = source;
        this.destination = destination;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        RenderTextureDescriptor Desc = renderingData.cameraData.cameraTargetDescriptor;
        Desc.width = Desc.width >> downSample;
        Desc.height = Desc.height >> downSample;
        Desc.depthBufferBits = 0;
        cmd.GetTemporaryRT(m_temporaryColorTexture01.id, Desc, filterMode);
        cmd.GetTemporaryRT(m_temporaryColorTexture02.id, Desc, filterMode);
        cmd.GetTemporaryRT(m_temporaryColorTexture03.id, Desc, filterMode);
       
        ///开始运算
        cmd.BeginSample("GaussianBlur");
        //将画面输入03
        cmd.Blit(source, m_temporaryColorTexture03.Identifier());
        //模糊次数循环
        for (int i = 0; i < blurCount; i++)
        {
            //输入纵向位移坐标
            m_VerticalBlurMat.SetVector("_offset", new Vector2(0, intensity));
            //将03的画面通过shader运算后输入01
            cmd.Blit(m_temporaryColorTexture03.Identifier(), m_temporaryColorTexture01.Identifier(), m_VerticalBlurMat);
            //输入横向位移坐标
            m_HorizontalBlurMat.SetVector("_offset", new Vector2(intensity, 0));
            //将01的画面通过shader运算后输入02
            cmd.Blit(m_temporaryColorTexture01.Identifier(), m_temporaryColorTexture02.Identifier(), m_HorizontalBlurMat);
            //将02的画面输入03
            cmd.Blit(m_temporaryColorTexture02.Identifier(), m_temporaryColorTexture03.Identifier());
        }
        //输出03到画面
        cmd.Blit(m_temporaryColorTexture03.Identifier(), source);
        cmd.EndSample("GaussianBlur");
        ///

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

