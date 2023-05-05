using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("锁定目标类型")]
    public enum TRAJECTORYITEM_TARGET_TYPE
    {
        [LabelText("最近距离目标")]
        NearestTarget = 0,
        [LabelText("最远距离目标")]
        FarestTarget = 1,
        [LabelText("随机目标")]
        RandomTarget = 2,
        [LabelText("最大血量的目标")]
        MaxHpTarget = 3,
        [LabelText("最小血量的目标")]
        MinHpTarget = 4,
        [LabelText("最大攻击的目标")]
        MaxAtkTarget = 5,
        [LabelText("最小攻击的目标")]
        MinAtkTarget = 6,
        [LabelText("当前目标")]
        CurTarget = 7,
    }
}