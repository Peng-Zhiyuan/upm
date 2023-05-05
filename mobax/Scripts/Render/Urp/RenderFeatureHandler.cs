using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderFeatureHandler : Instace<RenderFeatureHandler>
{
    public ScriptableRendererData RenderData;
    public void Clear()
    {
        this.RenderData = null;
    }
    private void Init()
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);// ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        RenderData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];
    }
    public ScriptableRendererFeature GetRenderFeatureByName (string featureName)
    {
        if (RenderData == null)
        {
            Init();
           // Debug.LogError("renders:"+RenderData.rendererFeatures.Count);
        }
        var feature = RenderData.rendererFeatures.Find(f => {
           // Debug.LogError("featureName:" + f.name);
            return f.name == featureName;
        });
        return feature;
    }
}
