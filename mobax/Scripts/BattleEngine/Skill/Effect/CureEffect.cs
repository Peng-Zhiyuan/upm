using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [Effect("治疗英雄", 20)]
    public class CureEffect : Effect
    {
        public override string Label => "治疗英雄";

        [ToggleGroup("Enabled"), LabelText("治疗数值(公式)"), LabelWidth(100)]
        public string CureValueFormula = "数值参数";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("治疗特效"), LabelWidth(80)]
        public string ParticleEffect;
    }
}