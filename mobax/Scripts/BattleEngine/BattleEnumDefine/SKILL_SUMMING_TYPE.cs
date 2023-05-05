using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("召唤位置类型")]
    public enum SKILL_SUMMING_TYPE
    {
        [LabelText("固定位置")]
        FixedPos = 0,
        [LabelText("固定范围内随机")]
        FixedAreaRandom,
        [LabelText("固定范围内规律")]
        FixedAreaFixed,
    }
}