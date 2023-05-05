using Sirenix.OdinInspector;

namespace SpineRegulate
{
    public enum ESpineTemplateType
    {
        [LabelText("无")] None = 0,
        [LabelText("全身像")] Model = 1,
        [LabelText("半身像")] HalfModel = 2,
        [LabelText("特写镜头")] FeatureCamera = 3,
    }
}