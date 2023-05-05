using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("屏幕震动", 5)]
    public sealed class ScreenShakeActionElement : SkillActionElementItem
    {
        public override string Label => "屏幕震动";
        [ToggleGroup("Enabled"), LabelText("震屏强度"), LabelWidth(100)]
        public float shakePower;
        [ToggleGroup("Enabled"), LabelText("震屏时长"), LabelWidth(100)]
        public float shakeTime;

        public override Color GetColor()
        {
            return Colors.ScreenShakeFrame;
        }
    }
}