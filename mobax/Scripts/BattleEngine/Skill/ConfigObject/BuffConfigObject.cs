using Sirenix.OdinInspector;
using System;

namespace BattleEngine.Logic
{
    [Serializable]
    public enum ConditionType
    {
        [LabelText("当x秒内没有受伤")]
        WhenInTimeNoDamage = 0,
        [LabelText("当生命值低于x")]
        WhenHPLower = 1,
        [LabelText("当生命值低于百分比x")]
        WhenHPPctLower = 2,
        [LabelText("当防御状态")]
        WhenDefense = 3,
        [LabelText("当暴怒状态")]
        WhenFury = 4,
    }
}