using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("技能生效检测方式")]
    public enum SKILL_JUDGEX_TYPE
    {
        [LabelText("自动不需要碰撞框")]
        Auto,
        [LabelText("手动指定")]
        PlayerSelect,
        [LabelText("条件指定")]
        ConditionSelect,
        [LabelText("固定区域场检测")]
        AreaSelect,
        [LabelText("目标位置-区域场检测")]
        TargetAreaSelect,
        [LabelText("预警位置-区域场检测")]
        PreWarningAreaSelect,
        [LabelText("目标方向-固定区域场检测")]
        TargetDirAreaSelect,
        [LabelText("输入位置-固定区域场检测")]
        InputAreaSelect,
    }
}