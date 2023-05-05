using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable]
    public enum UIBlurQuality
    {
        None,
        _2X,
        _4X,
    }

    [Serializable]
    public sealed class QualityParameter : VolumeParameter<UIBlurQuality>
    {
        public QualityParameter(UIBlurQuality value, bool overrideState = false) : base(value, overrideState) { }
    }

    [Serializable, VolumeComponentMenu("QD/UIBlur")]
    public class UIBlur : VolumeComponent, IPostProcessComponent
    {
        [Tooltip("The quality of the UIBlur.")]
        public QualityParameter downSample = new QualityParameter(UIBlurQuality._4X,false);

        [Tooltip("Strength of the UIBlur filter.")]
        public ClampedFloatParameter m_BlurIntensity = new ClampedFloatParameter(0.0f, 0.0f, 10.0f);
        public bool IsActive() => m_BlurIntensity.value > 0.0f;
        public bool IsTileCompatible() => true;
    }
}
