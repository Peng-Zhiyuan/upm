namespace BattleEngine.Logic
{
    using Sirenix.OdinInspector;
    using System;
    
    [Serializable]
    [LabelText("锁定目标类型")]
    public enum LOCK_DOWN_TARGET_TYPE
    {
        [LabelText("无")]
        None = 0,
        [LabelText("当前目标")]
        CurrentTarget = 1,
        [LabelText("最近距离目标")]
        NearestTarget = 2,
        [LabelText("最远距离目标")]
        FarestTarget = 3,
        [LabelText("最大血量的目标")]
        MaxHpTarget = 4,
        [LabelText("最小血量的目标")]
        MinHpTarget = 5,
        [LabelText("最大攻击的目标")]
        MaxAtkTarget = 6,
        [LabelText("最小攻击的目标")]
        MinAtkTarget = 7,
        [LabelText("最大防御的目标")]
        MaxDefTarget = 8,
        [LabelText("最小防御的目标")]
        MinDefTarget = 9,
        [LabelText("玩家选中目标")]
        SelectTargetSelf = 100,
        [LabelText("玩家选中目标的目标")]
        SelectTarget = 101
    }
}