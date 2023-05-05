using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NGGrabTexturePass
{
    private ShaderTagId mGrabTextureShaderTag;
    private RenderTexture grabTexture = null;
    private RenderTargetIdentifier grabTextureIdentifier;
    private NGRenderBuffer ngRenderBuffer;
    private FilteringSettings mGrabFilter;

    public NGGrabTexturePass()
    {
        mGrabTextureShaderTag = new ShaderTagId("GrabTexture");
        RenderQueueRange queue = new RenderQueueRange();
        queue.lowerBound = 2000;
        queue.upperBound = 3000;
        mGrabFilter = new FilteringSettings(queue);
    }

    public void Setup(NGRenderBuffer ngRenderBuffer)
    {
        this.ngRenderBuffer = ngRenderBuffer;
    }

    public void Execute(ScriptableRenderPass pass,ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //CameraCustomData customData = renderingData.cameraData.camera.transform.GetComponent<CameraCustomData>();
       // if (customData == null || (customData.enabled && !customData.EnableGrabTexture))
       //     return;
        if(NGGlobalSettings.GrabObjectsCount > 0)
        {
            //Debug.LogError("GrabObjectsCount"+ NGGlobalSettings.GrabObjectsCount);
            RenderTextureDescriptor rtDescBase = renderingData.cameraData.cameraTargetDescriptor;
            grabTexture = RenderTexture.GetTemporary(rtDescBase);
            grabTextureIdentifier = grabTexture;
            CommandBuffer cmd = CommandBufferPool.Get("GrabTexture");
            cmd.Blit(ngRenderBuffer.GetSource(), grabTextureIdentifier);
            cmd.SetGlobalTexture("_GrabTexture", grabTextureIdentifier);
            // cmd.SetGlobalTexture("_CameraOpaqueTexture", grabTextureIdentifier);
            //cmd.SetRenderTarget(ngRenderBuffer.GetSource());
            context.ExecuteCommandBuffer(cmd);

            //DrawingSettings grabTextureSetting = pass.CreateDrawingSettings(mGrabTextureShaderTag, ref renderingData, SortingCriteria.CommonTransparent);
            //context.DrawRenderers(renderingData.cullResults, ref grabTextureSetting,ref mGrabFilter);
                                
            CommandBufferPool.Release(cmd);
            RenderTexture.ReleaseTemporary(grabTexture);
            grabTexture = null;
        }
    }
}
