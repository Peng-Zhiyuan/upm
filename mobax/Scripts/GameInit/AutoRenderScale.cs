using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
public class AutoRenderScale : MonoInstance<AutoRenderScale>
{
    public float minScale = 0.7f;
    public bool autoScale = true;
    public UniversalRenderPipelineAsset urpAsset;
    private void Awake()
    {
        urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        urpAsset.renderScale = 1;
    }
   
    void Update()
    {
        if (!autoScale || urpAsset == null) return;
        var fps = 1.0f / Time.smoothDeltaTime;
        Debug.Log(fps / DeveloperLocalSettings.FPS);
        urpAsset.renderScale = Mathf.Clamp(minScale, 1, fps / DeveloperLocalSettings.FPS);
    }

}
