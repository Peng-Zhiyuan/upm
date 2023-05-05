namespace BattleEngine.Logic
{
    using Sirenix.OdinInspector;

    [Effect("触发技能", 100)]
    public class TriggerSkillEffect : Effect
    {
        public override string Label => "触发技能";

        [ToggleGroup("Enabled"), LabelText("技能ID")]
        public uint SkillId;
    }
}