using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("锁定位置类型")]
    public enum TRAJECTORYITEM_POS_TYPE
    {
        [LabelText("固定距离（以飞行最大距离）")]
        DistancePos = 0,
        [LabelText("最近距离目标")]
        NearestTargetPos = 0,
        [LabelText("最远距离目标")]
        FarestTargetPos = 1,
        [LabelText("随机目标")]
        RandomTargetPos = 2,
        [LabelText("最大血量的目标")]
        MaxHpTargetPos = 3,
        [LabelText("最小血量的目标")]
        MinHpTargetPos = 4,
        [LabelText("当前目标")]
        CurTargetPos = 5,
    }
}