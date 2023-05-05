using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("作用对象")]
    public enum SKILL_MOVE_TARGET_TYPE
    {
        [LabelText("技能目标")]
        SkillTarget = 1,
        [LabelText("自身方向")]
        Self = 2,
        [LabelText("技能多目标最近")]
        SkillTargetMin = 3,
        [LabelText("技能多目标最远")]
        SkillTargetMax = 4,
        [LabelText("输入位置")]
        MoveInputPos = 5
    }

    [System.Serializable]
    [LabelText("作用对象")]
    public enum SKILL_BEATBACK_TYPE
    {
        [LabelText("技能目标")]
        SkillTarget = 1,
        [LabelText("全局")]
        Global = 2
    }
    
    [System.Serializable]
    [LabelText("作用对象")]
    public enum SKILL_JUMP_TARGET_TYPE
    {
        [LabelText("技能目标")]
        SkillTarget = 1,
        [LabelText("自身方向")]
        Self = 2,
    }
}