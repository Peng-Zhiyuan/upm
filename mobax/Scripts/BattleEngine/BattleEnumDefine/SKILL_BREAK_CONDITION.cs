using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    /// <summary>
    /// 技能中断条件
    /// </summary>
    public enum SKILL_BREAK_CONDITION
    {
        [LabelText("无条件")]
        None = 0,
        [LabelText("移动")]
        Move,
        [LabelText("正常受控状态，指被攻击时")]
        NormalControlled,
        [LabelText("Buff受控状态，指添加控制类buff时")]
        BuffControlled,
        [LabelText("技能没有命中时")]
        JudgeMissed
    }
}