using Sirenix.OdinInspector;
using System;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [Flags]
    [LabelText("技能打断指令")]
    public enum SKILL_BREAK_CAUSE
    {
        None = 0,
        [LabelText("手动移动")]
        ManualMove = 1 << 1,
        [LabelText("大招")]
        UltraSkill = 1 << 2,
        [LabelText("手动攻击")]
        ManualAttack = 1 << 3,
        [LabelText("集火")]
        Focus = 1 << 4,
        [LabelText("自动移动")]
        AutoMove = 1 << 5,
        [LabelText("自动攻击")]
        AutoAttack = 1 << 6,
        [LabelText("位移技能")]
        MoveAttack = 1 << 7
    }
}