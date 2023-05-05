using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    
    /// <summary>
    /// 技能大类型
    /// </summary>
    public enum SKILL_TYPE
    {
        [LabelText("被动技能")]
        Passive = 0,
        [LabelText("普攻")]
        ATK = 1,
        [LabelText("技能")]
        SSP = 2,
        [LabelText("大招")]
        SPSKL = 3,
        [LabelText("战术技能")]
        ItemSKL = 4,
        [LabelText("上场技能")]
        SSPMove = 5,
        [LabelText("关卡被动技")]
        STAGE_PASSIVE_SKILL = 6,
        [LabelText("搜索被动")]
        EXPLORER = 10,
        
    }
}