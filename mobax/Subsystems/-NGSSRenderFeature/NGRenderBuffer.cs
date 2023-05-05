using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NGRenderBuffer
{
    private RenderTargetIdentifier source;
    private bool mUseCustomSource = false;
    private RenderTexture rtCustomSourceTexture;
    private RenderTargetIdentifier rtCustomSource;

    public void Setup(RenderTargetIdentifier source)
    {
        this.source = source;
    }

    public RenderTargetIdentifier GetSource()
    {
        if (mUseCustomSource)
        {
            return rtCustomSource;
        }
        else
        {
            return source;
        }
    }

    public RenderTargetIdentifier GetTarget()
    {
        if (mUseCustomSource)
        {
            return source;
        }
        else
        {
            return rtCustomSource;
        }
    }
 
    public void InitCustomSource(RenderingData renderingData)
    {
        mUseCustomSource = false;
        RenderTextureDescriptor rtDescBase = renderingData.cameraData.cameraTargetDescriptor;
        rtDescBase.msaaSamples = 1;
        rtDescBase.depthBufferBits = 0;
        rtCustomSourceTexture = RenderTexture.GetTemporary(rtDescBase);
        rtCustomSource = rtCustomSourceTexture;
    }

    public void SwapBuffer()
    {
        mUseCustomSource = !mUseCustomSource;
    }

    public void ResetBuffer(ScriptableRenderContext context, CommandBuffer cmd)
    {
        if (mUseCustomSource)
        {
            cmd.Blit(rtCustomSource, source);
            context.ExecuteCommandBuffer(cmd);
            mUseCustomSource = !mUseCustomSource;
        }
       
    }
    public void FinalCopy(ScriptableRenderContext context, CommandBuffer cmd)
    {
        if (mUseCustomSource)
        {
            cmd.Blit(rtCustomSource, source);
            context.ExecuteCommandBuffer(cmd);
        }
        if (rtCustomSourceTexture != null)
        {
            RenderTexture.ReleaseTemporary(rtCustomSourceTexture);
            rtCustomSourceTexture = null;
        }
    }
}