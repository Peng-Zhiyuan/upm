using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace BattleEngine.Logic
{
    [Serializable]
    [SkillActionElementItem("击退", 23)]
    public sealed class BeatBackActionElement : SkillActionElementItem
    {
        public override string Label => "击退";
        [ToggleGroup("Enabled"), LabelText("目标选定")]
        [LabelWidth(80)]
        public SKILL_BEATBACK_TYPE beatBackType = SKILL_BEATBACK_TYPE.SkillTarget;
        [ToggleGroup("Enabled"), LabelText("是否自由方向"), LabelWidth(100)]
        public bool freeDir;
        [ToggleGroup("Enabled"), LabelText("最大距离"), LabelWidth(80)]
        public float moveMaxDis = 10;

        public override Color GetColor()
        {
            return Colors.BeatBackFrame;
        }
    }
}