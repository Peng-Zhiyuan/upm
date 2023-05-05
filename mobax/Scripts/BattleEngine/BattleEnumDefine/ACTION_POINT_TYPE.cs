using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("行动点类型")]
    public enum ACTION_POINT_TYPE
    {
        [LabelText("造成伤害前")]
        PreCauseDamage,
        [LabelText("承受伤害前")]
        PreReceiveDamage,
        [LabelText("造成伤害后")]
        PostCauseDamage,
        [LabelText("承受伤害后")]
        PostReceiveDamage,
        [LabelText("给予治疗前")]
        PreGiveCure,
        [LabelText("接受治疗前")]
        PreReceiveCure,
        [LabelText("给予治疗后")]
        PostGiveCure,
        [LabelText("接受治疗后")]
        PostReceiveCure,
        [LabelText("赋加状态后")]
        PostGiveStatus,
        [LabelText("承受状态后")]
        PostReceiveStatus,
        [LabelText("普通攻击后")]
        PostNormalAtk,
        [LabelText("破防状态")]
        PostBreakStatus,
        [LabelText("击杀状态")]
        PostKillHeroStatus,
        [LabelText("死亡状态")]
        PostDeadStatus,
        Max,
    }
}