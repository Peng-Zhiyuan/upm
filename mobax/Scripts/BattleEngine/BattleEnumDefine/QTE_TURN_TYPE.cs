using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("转向朝向类型")]
    public enum QTE_TURN_FORWARD_TYPE
    {
        [LabelText("目标位置")]
        Target,
        [LabelText("预警位置")]
        Prewaring
    }

    [System.Serializable]
    [LabelText("转向部位类型")]
    public enum QTE_TURN_BIND_TYPE
    {
        [LabelText("根节点")]
        Root,
        [LabelText("部位节点")]
        Party
    }
}