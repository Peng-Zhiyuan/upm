using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("弹道类型")]
    public enum TRAJECTORYITEM_TYPE
    {
        [LabelText("自由弹道")]
        Free = 0,
        [LabelText("选择目标弹道")]
        SelectTarget,
        [LabelText("选择位置弹道")]
        SelectPos,
        [LabelText("飞出返回弹道")]
        Back,
        [LabelText("抛物线")]
        Parabola
    }
}