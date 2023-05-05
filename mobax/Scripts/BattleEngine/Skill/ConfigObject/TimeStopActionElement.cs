using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("时停(停用)", 14)]
    public sealed class TimeStopActionElement : SkillActionElementItem
    {
        public override string Label => "时停(停用)";

        public override Color GetColor()
        {
            return Colors.TimeStopFrame;
        }
    }
}