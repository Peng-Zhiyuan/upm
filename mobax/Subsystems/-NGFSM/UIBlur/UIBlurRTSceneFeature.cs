using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIBlurRTSceneFeature : ScriptableRendererFeature
{
    public UIBlurRTSceneSetting m_Setting = new UIBlurRTSceneSetting();
    UIBlurRTScenePass m_Pass;
    UIBlur m_UIBlur;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //m_UIBlur = VolumeManager.instance.stack.GetComponent<UIBlur>();
        //if (m_UIBlur != null && m_UIBlur.IsActive())
        renderer.EnqueuePass(m_Pass);

    }

    public override void Create()
    {
        m_Pass = new UIBlurRTScenePass(m_Setting);
    }
}

[System.Serializable]
public class UIBlurRTSceneSetting
{
    public RenderPassEvent renderPassEvent;
    public Shader m_Shader;
    public UIBlurQuality downSample;
    [Range(0, 20f)]
    public float m_BlurIntensity;
}
public enum UIBlurQuality
{
    None,
    _2X,
    _4X,
}