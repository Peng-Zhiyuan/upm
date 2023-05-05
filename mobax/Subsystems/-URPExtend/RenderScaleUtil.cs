using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class RenderScaleUtil : Single<RenderScaleUtil>
{
    //[SerializeField]
    private UniversalRenderPipelineAsset pipelineAsset;
    private UniversalRenderPipelineAsset PipelineAsset
    {
        get
        {
            if (pipelineAsset == null)
            {
                pipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            }
            return pipelineAsset;
        }

    }
    
    public void SetRenderScale(float value)
    {
        if (PipelineAsset != null)
        {
            PipelineAsset.renderScale = value;
        }
    }

  
}
