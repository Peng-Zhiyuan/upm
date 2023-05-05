using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class FullScreenMultiplyPass : ScriptableRenderPass
{
    protected RenderTextureDescriptor rtDesc;
    public int tempTex;
    public RenderTargetIdentifier sourceTex;
    protected Material material;
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        rtDesc = cameraTextureDescriptor;

    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("FullScreenMultiply");
        cmd.GetTemporaryRT(tempTex, rtDesc);
        cmd.Blit(sourceTex, tempTex, material);
        cmd.Blit(tempTex, sourceTex);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Setup(RenderTargetIdentifier cameraColorTex, Material mat)
    {
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        sourceTex = cameraColorTex;
        material = mat;
    }

    public override void OnFinishCameraStackRendering(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTex);
        base.OnFinishCameraStackRendering(cmd);
    }

}

public class FullScreenMultiplyFeature : ScriptableRendererFeature
{
    public static FullScreenMultiplyFeature instance;
    [System.Serializable]
    public class FilterSettings
    {
        public LayerMask LayerMask;
        public FilterSettings()
        {
            LayerMask = 0;
        }
    }


    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public FilterSettings filterSettings = new FilterSettings();
        public Shader shader;
        [Range(0f, 1f)]
        public float value;
        public float lerpSpeed = 8f;
        [HideInInspector]
        public float targetVal = 1f;
    }


    public Settings settings = new Settings();
    protected FullScreenMultiplyPass pass;
    protected Material material;

    protected bool lastFramePlaying = false;
    public override void Create()
    {
        pass = new FullScreenMultiplyPass();
        material = CoreUtils.CreateEngineMaterial(settings.shader);
        instance = this;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (Application.isPlaying != lastFramePlaying)
        {
            settings.value = 1f;
            settings.targetVal = 1f;
        }
        if (Application.isPlaying)
        {
            settings.value = Mathf.MoveTowards(settings.value, settings.targetVal, Time.deltaTime * settings.lerpSpeed);
        }
        if (settings.value < 1f)
        {
            material.SetFloat("_Val", settings.value);
            pass.Setup(renderer.cameraColorTarget, material);
            renderer.EnqueuePass(pass);
        }
        lastFramePlaying = Application.isPlaying;
    }
}
