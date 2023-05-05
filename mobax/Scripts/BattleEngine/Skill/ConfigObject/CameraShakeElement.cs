using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("震屏", 28)]
    public sealed class CameraShakeElement : SkillActionElementItem
    {
        public override string Label => "震屏";

        [ToggleGroup("Enabled"), LabelText("振动时长")]
        [LabelWidth(100)]
        public float Duration;

        [ToggleGroup("Enabled"), LabelText("振幅")]
        [LabelWidth(100)]
        public float ShakeIntensity;

        [ToggleGroup("Enabled"), LabelText("频率")]
        [LabelWidth(100)]
        public float Frenquence;

        public override Color GetColor()
        {
            return Colors.ScreenShakeFrame;
        }
    }
}