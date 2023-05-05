using Sirenix.OdinInspector;

namespace SpineRegulate
{
    public enum ESpineRegulateType
    {
        [LabelText("无")] None = 0,
        [LabelText("模板数据")] Template = 1,
        [LabelText("指定数据")] Appoint = 2,
    }
}