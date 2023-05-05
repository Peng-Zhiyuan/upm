using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



public enum RenderQueueType
{
    Opaque,
    Transparent,
}

public class PlanerShadowFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class RenderObjectsSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public FilterSettings filterSettings = new FilterSettings();

        public Material overrideMaterial = null;
        public int overrideMaterialPassIndex = 0;

        public bool overrideDepthState = false;
        public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

    }

    [System.Serializable]
    public class FilterSettings
    {
        // TODO: expose opaque, transparent, all ranges as drop down
        public RenderQueueType RenderQueueType;
        public LayerMask LayerMask;
        public string[] PassNames;

        public FilterSettings()
        {
            RenderQueueType = RenderQueueType.Opaque;
            LayerMask = 0;
        }
    }

    [System.Serializable]
    public class CustomCameraSettings
    {
        public bool overrideCamera = false;
        public bool restoreCamera = true;
        public Vector4 offset;
        public float cameraFieldOfView = 60.0f;
    }

    public RenderObjectsSettings settings = new RenderObjectsSettings();

    PlanerShadowPass renderObjectsPass;

    public override void Create()
    {
        FilterSettings filter = settings.filterSettings;
        renderObjectsPass = new PlanerShadowPass(settings.Event, filter.PassNames,
            filter.RenderQueueType, filter.LayerMask);

        renderObjectsPass.overrideMaterial = settings.overrideMaterial;
        renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderObjectsPass);
    }
}




