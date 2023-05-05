using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace BattleEngine.Logic
{
    [Serializable]
    [SkillActionElementItem("事件", 9)]
    public sealed class BattleEventActionElement : SkillActionElementItem
    {
        public override string Label => "事件";
        [ToggleGroup("Enabled"), LabelText("事件类型"), LabelWidth(100)]
        public SKILL_EVENT_TYPE eventType = SKILL_EVENT_TYPE.NONE;
        [ToggleGroup("Enabled"), LabelText("事件参数1"), LabelWidth(100)]
        public string param = "";
        [ToggleGroup("Enabled"), LabelText("事件参数2"), LabelWidth(100)]
        public int param2 = 0;
        [ToggleGroup("Enabled"), LabelText("事件参数3"), LabelWidth(100)]
        public int param3 = 0;
        [ToggleGroup("Enabled"), LabelText("事件参数4"), LabelWidth(100)]
        public int param4 = 0;
        [ToggleGroup("Enabled"), LabelText("颜色参数"), LabelWidth(100)]
        public string hexColor;

        public override Color GetColor()
        {
            return Colors.BattleEventFrame;
        }
    }
}