using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("屏幕压暗", 12)]
    public sealed class ScreenDimmingActionElement : SkillActionElementItem
    {
        public override string Label => "屏幕压暗";

        public override Color GetColor()
        {
            return Colors.ScreenDimmingFrame;
        }
    }
}