using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("召唤", 21)]
    public sealed class SummoningActionElement : SkillActionElementItem
    {
        public override string Label => "召唤";
        [ToggleGroup("Enabled"), LabelText("召唤物ID"), LabelWidth(100)]
        public int summoningActorId = 1;
        [ToggleGroup("Enabled"), LabelText("召唤物等级"), LabelWidth(100)]
        public int summoningActorLv = 1;
        [ToggleGroup("Enabled"), LabelText("召唤物出生位置"), LabelWidth(100)]
        public List<Vector3> summoningActorPos = new List<Vector3>() { Vector3.zero };
        [ToggleGroup("Enabled"), LabelText("召唤物存在时间"), LabelWidth(100), SuffixLabel("毫秒，-1永久", true), HideLabel]
        public int summoningActorLifeTime = -1;

        public override Color GetColor()
        {
            return Colors.SummoningFrame;
        }
    }
}