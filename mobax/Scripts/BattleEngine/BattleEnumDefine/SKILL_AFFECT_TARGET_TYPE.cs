using System;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [Serializable]
    [LabelText("技能目标类型")]
    public enum SKILL_AFFECT_TARGET_TYPE
    {
        [LabelText("自己")]
        Self = 0,
        [LabelText("队友")]
        Team = 2,
        [LabelText("敌人")]
        Enemy = 3,
        [LabelText("所有")]
        All = 4
    }
}