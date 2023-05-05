using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("吸星大法", 29)]
    public sealed class TargetTogetherActionElement : SkillActionElementItem
    {
        public override string Label => "吸星大法";
        [ToggleGroup("Enabled"), LabelText("位置偏移"),LabelWidth(120)]
        public Vector3 offset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("角度偏移"),LabelWidth(120)]
        public Vector3 angleOffset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("吸引类型")]
        [LabelWidth(80)]
        public SKILL_TOGETHER_TYPE togetherType = SKILL_TOGETHER_TYPE.FREE;

        /// <summary>
        /// 吸星-滞空
        /// </summary>
        private bool isHoverTogether
        {
            get { return togetherType == SKILL_TOGETHER_TYPE.HOVER; }
        }
        [ToggleGroup("Enabled"), LabelText("上升高度"), LabelWidth(120), ShowIf("isHoverTogether")]
        public float Hover_High = 0.0f;
        [ToggleGroup("Enabled"), LabelText("上升时间(帧)"), LabelWidth(120), ShowIf("isHoverTogether")]
        public int Hover_Up_Time = 0;
        [ToggleGroup("Enabled"), LabelText("上升曲线"), LabelWidth(120), ShowIf("isHoverTogether")]
        public AnimationCurve Hover_verCureUp;
        [ToggleGroup("Enabled"), LabelText("滞空时间(帧)"), LabelWidth(120), ShowIf("isHoverTogether")]
        public int Hover_Time = 0;
        [ToggleGroup("Enabled"), LabelText("下落时间(帧)"), LabelWidth(120), ShowIf("isHoverTogether")]
        public int Hover_Down_Time = 0;
        [ToggleGroup("Enabled"), LabelText("下落曲线"), LabelWidth(120), ShowIf("isHoverTogether")]
        public AnimationCurve Hover_verCureDown;

        public override Color GetColor()
        {
            return Colors.BeatBackFrame;
        }
    }
}