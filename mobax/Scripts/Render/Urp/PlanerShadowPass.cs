using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine;

public class PlanerShadowPass : ScriptableRenderPass
{
    RenderQueueType renderQueueType;
    FilteringSettings m_FilteringSettings;
    string m_ProfilerTag = "PlanerShadow";
    ProfilingSampler m_ProfilingSampler;

    public Material overrideMaterial { get; set; }
    public int overrideMaterialPassIndex { get; set; }

    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
    {
        m_RenderStateBlock.mask |= RenderStateMask.Depth;
        m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
    }

    public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
    {
        StencilState stencilState = StencilState.defaultValue;
        stencilState.enabled = true;
        stencilState.SetCompareFunction(compareFunction);
        stencilState.SetPassOperation(passOp);
        stencilState.SetFailOperation(failOp);
        stencilState.SetZFailOperation(zFailOp);

        m_RenderStateBlock.mask |= RenderStateMask.Stencil;
        m_RenderStateBlock.stencilReference = reference;
        m_RenderStateBlock.stencilState = stencilState;
    }

    RenderStateBlock m_RenderStateBlock;

    public PlanerShadowPass(RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask)
    {
  
        m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        this.renderPassEvent = renderPassEvent;
        this.renderQueueType = renderQueueType;
        this.overrideMaterial = null;
        this.overrideMaterialPassIndex = 0;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);

        if (shaderTags != null && shaderTags.Length > 0)
        {
            foreach (var passName in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }
        else
        {
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
            ? SortingCriteria.CommonTransparent
            : renderingData.cameraData.defaultOpaqueSortFlags;

        DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
        drawingSettings.overrideMaterial = overrideMaterial;
        drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;

        /*
        ref CameraData cameraData = ref renderingData.cameraData;
        Camera camera = cameraData.camera;
        */

        // In case of camera stacking we need to take the viewport rect from base camera
       // Rect pixelRect = renderingData.cameraData.pixelRect;
        //float cameraAspect = (float)pixelRect.width / (float)pixelRect.height;
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                ref m_RenderStateBlock);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);
    }
    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
    }
}

 