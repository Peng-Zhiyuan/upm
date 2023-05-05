namespace BattleEngine.Logic
{
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Effect("造成伤害", 10)]
    public class DamageEffect : Effect
    {
        public override string Label => "造成伤害";

        [ToggleGroup("Enabled"), LabelWidth(80)]
        public DAMAGE_TYPE DamageType;

        [TextArea, ToggleGroup("Enabled"), LabelText("伤害数值(公式)"), LabelWidth(100)]
        public string DamageValueFormula = "数值参数";
    }
}