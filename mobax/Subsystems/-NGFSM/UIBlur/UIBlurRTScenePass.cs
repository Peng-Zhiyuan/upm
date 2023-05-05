using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIBlurRTScenePass : ScriptableRenderPass
{
    private UIBlurRTSceneSetting m_Setting;
    private Material material;
    ProfilingSampler m_ProfilingSample;
    private RenderTargetHandle m_UIBlurTexture;
    public UIBlurRTScenePass(UIBlurRTSceneSetting m_Setting)
    {
        this.m_Setting = m_Setting;
        renderPassEvent = m_Setting.renderPassEvent;
        if (m_Setting.m_Shader!=null && material==null)
        {
            material = new Material(m_Setting.m_Shader);
        }
        m_ProfilingSample = new ProfilingSampler(nameof(UIBlurRTScenePass));
        m_UIBlurTexture.Init("UI Blur RT");
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        base.OnCameraSetup(cmd, ref renderingData);
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        if (m_Setting.downSample == UIBlurQuality._2X)
        {
            desc.width /= 2;
            desc.height /= 2;
        }
        else if (m_Setting.downSample == UIBlurQuality._4X)
        {
            desc.width /= 4;
            desc.height /= 4;
        }
        desc.colorFormat = RenderTextureFormat.ARGB32;
        cmd.GetTemporaryRT(m_UIBlurTexture.id, desc,FilterMode.Bilinear);
        ConfigureTarget(m_UIBlurTexture.id);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null)
            return;
        CommandBuffer cmd = CommandBufferPool.Get(nameof(UIBlurRTScenePass));
        using (new ProfilingScope(cmd, m_ProfilingSample))
        {
            cmd.SetGlobalTexture("_MainTex", renderingData.cameraData.renderer.cameraColorTarget);
            material.SetFloat("_BlurIntensity",m_Setting.m_BlurIntensity);
            //cmd.DrawMesh(RenderingUtils.fullscreenTriangle,Matrix4x4.identity, material,0);
            cmd.Blit(m_UIBlurTexture.id, renderingData.cameraData.renderer.cameraColorTarget);
            //顺序 后处理之后,Blit到主ColorRT
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
        cmd.ReleaseTemporaryRT(m_UIBlurTexture.id);
    }
}
