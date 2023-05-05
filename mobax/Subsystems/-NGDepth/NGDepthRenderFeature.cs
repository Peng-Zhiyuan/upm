using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
[System.Serializable]
public class NGDepthSettings
{
    public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;
}
public class NGDepthRenderFeature : ScriptableRendererFeature
{

    public NGDepthSettings settings = new NGDepthSettings();

    protected NGDepthPass depthPass;
    public override void Create()
    {
        depthPass = new NGDepthPass(settings, settings.passEvent);// RenderPassEvent.AfterRenderingOpaques);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        depthPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(depthPass);
    }
}

public class NGDepthPass : ScriptableRenderPass
{
  
    static readonly string k_RenderTag = "NGDepthPass";
    //static readonly int TempTargetId = Shader.PropertyToID("_TempTarget");
    //static readonly int MainTexId = Shader.PropertyToID("_MainTex");

    /// <summary>
    /// 深度图id
    /// </summary>
    static readonly int m_DepthTexId = Shader.PropertyToID("_DepthTex");

    /// <summary>
    /// 用于存储深度
    /// </summary>
    private RenderTexture m_depthBufferTex;

    /// <summary>
    /// 深度图
    /// </summary>
    private RenderTexture m_DepthTex;

    /// <summary>
    /// 在渲染天空盒之后的commandbuff指令
    /// </summary>
    //private CommandBuffer m_AfterSkyboxCommandBuffer;

    /// <summary>
    /// 处理深度图的commandbuff指令
    /// </summary>
    //private CommandBuffer m_DepthBuffer;

    public NGDepthSettings mDepthSettings;
    RenderTargetIdentifier currentTarget;

    public NGDepthPass(NGDepthSettings settings,RenderPassEvent evt)
    {
        renderPassEvent = evt;
        mDepthSettings = settings;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //添加处理深度图commandbuffer
        var cmd = CommandBufferPool.Get(k_RenderTag);
        //存储深度
        //m_depthBufferTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        //m_depthBufferTex.name = "DepthBuffer";
        RenderTextureDescriptor renderTextureDescriptor;
        cmd.GetTemporaryRT(m_DepthTexId, Screen.width, Screen.height, 24, FilterMode.Point, RenderTextureFormat.Depth);


        //深度图
        m_DepthTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RHalf);
        m_DepthTex.name = "DepthTex";
        //m_DepthTex = RenderTexture.GetTemporary(m_DepthTexId);

        //m_DepthBuffer = new CommandBuffer();
        //m_DepthBuffer.name = "CommandBuffer_DepthBuffer";
        //把depthbuffer写入m_DepthTex的colorbuffer
        //把depthbuffer合成一张rt和自带的是重新渲染一张rt效果一样
        //我这里定义rt全局id为_DepthTex，shader直接获取这个就可以使用自定义深度图
        //cmd.Blit(m_depthBufferTex.depthBuffer, m_DepthTex.colorBuffer);
        //cmd.Blit(m_DepthTexId, this.currentTarget);
        //m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthBuffer);
        //设置shader全局深度图
        context.ExecuteCommandBuffer(cmd);
        //Shader.SetGlobalTexture(m_DepthTexId, m_DepthTex);
        CommandBufferPool.Release(cmd);
    }

    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }
    /*
    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var source = currentTarget;
        int destination = TempTargetId;

        var w = cameraData.camera.scaledPixelWidth;
        var h = cameraData.camera.scaledPixelHeight;
        int shaderPass = 0;
        cmd.SetGlobalTexture(MainTexId, source);
        cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
        cmd.Blit(source, destination);
        //cmd.Blit(destination, source, mEffSettings.NGDepthMaterial, shaderPass);
    }
    */
}