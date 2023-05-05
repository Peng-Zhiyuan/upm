using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("跳跃", 24)]
    public sealed class JumpActionElement : SkillActionElementItem
    {
        public override string Label => "跳跃";
        [ToggleGroup("Enabled"), LabelText("目标选定")]
        [LabelWidth(80)]
        public SKILL_JUMP_TARGET_TYPE MoveTargetType = SKILL_JUMP_TARGET_TYPE.SkillTarget;
        public bool IsNoneTarget
        {
            get { return MoveTargetType == SKILL_JUMP_TARGET_TYPE.Self; }
        }
        [ToggleGroup("Enabled"), LabelText("跳跃高度"), LabelWidth(120)]
        public float verHeight;
        [ToggleGroup("Enabled"), LabelText("跳跃距离"), LabelWidth(120), ShowIf("IsNoneTarget")]
        public float verDistance;
        [ToggleGroup("Enabled"), LabelText("垂直曲线"), LabelWidth(120)]
        public AnimationCurve verCure;
        [ToggleGroup("Enabled"), LabelText("水平曲线"), LabelWidth(120)]
        public AnimationCurve horCure;

        public override Color GetColor()
        {
            return Colors.JumpFrame;
        }
    }
}