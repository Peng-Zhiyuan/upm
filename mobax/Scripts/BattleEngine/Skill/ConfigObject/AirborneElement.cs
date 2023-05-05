using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [Serializable]
    [SkillActionElementItem("击飞", 25)]
    public sealed class AirBorneElement : SkillActionElementItem
    {
        public override string Label => "击飞";
        [ToggleGroup("Enabled"), LabelText("击飞距离"), LabelWidth(120)]
        public SKILL_AIRBORNE_TYPE FlyType = SKILL_AIRBORNE_TYPE.FREE;

        public bool isFree
        {
            get { return FlyType == SKILL_AIRBORNE_TYPE.FREE; }
        }
        public bool isHover
        {
            get { return FlyType == SKILL_AIRBORNE_TYPE.HOVER; }
        }

        [ToggleGroup("Enabled"), LabelText("击飞距离"), LabelWidth(120), ShowIf("isFree")]
        public float Free_Distance = 0.0f;
        [ToggleGroup("Enabled"), LabelText("水平曲线"), LabelWidth(120), ShowIf("isFree")]
        public AnimationCurve Free_horCure;
        [ToggleGroup("Enabled"), LabelText("击飞高度"), LabelWidth(120), ShowIf("isFree")]
        public float Free_High = 0.0f;
        [ToggleGroup("Enabled"), LabelText("垂直曲线"), LabelWidth(120), ShowIf("isFree")]
        public AnimationCurve Free_verCure;

        [ToggleGroup("Enabled"), LabelText("上升高度"), LabelWidth(120), ShowIf("isHover")]
        public float Hover_High = 0.0f;
        [ToggleGroup("Enabled"), LabelText("上升时间(帧)"), LabelWidth(120), ShowIf("isHover")]
        public int Hover_Up_Time = 0;
        [ToggleGroup("Enabled"), LabelText("上升曲线"), LabelWidth(120), ShowIf("isHover")]
        public AnimationCurve Hover_verCureUp;
        [ToggleGroup("Enabled"), LabelText("滞空时间(帧)"), LabelWidth(120), ShowIf("isHover")]
        public int Hover_Time = 0;
        [ToggleGroup("Enabled"), LabelText("下落时间(帧)"), LabelWidth(120), ShowIf("isHover")]
        public int Hover_Down_Time = 0;
        [ToggleGroup("Enabled"), LabelText("下落曲线"), LabelWidth(120), ShowIf("isHover")]
        public AnimationCurve Hover_verCureDown;

        public override Color GetColor()
        {
            return Colors.AirBorneFrame;
        }
    }
}