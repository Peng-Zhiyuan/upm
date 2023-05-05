using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("预警位置")]
    public enum SKILL_PRE_WARING_TYPE
    {
        [LabelText("固定位置")]
        FixedPosition,
        [LabelText("随机位置")]
        Random,
        [LabelText("随机目标位置")]
        RandomTarget,
        [LabelText("当前目标位置")]
        CurTarget,
    }

    [System.Serializable]
    [LabelText("预警类型")]
    public enum SKILL_PRE_WARING_EFFECT_TYPE
    {
        [LabelText("圆形")]
        Circle,
        [LabelText("箭形")]
        Ray,
        [LabelText("矩形")]
        Retangle,
    }
}