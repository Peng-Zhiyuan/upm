namespace BattleEngine.Logic
{
    using Sirenix.OdinInspector;

    //[Effect("属性数值修饰", 50)]
    public class AttributeNumericModifyEffect : Effect
    {
        public override string Label => "属性数值修饰";

        [ToggleGroup("Enabled")]
        public AttrType NumericType;

        [ToggleGroup("Enabled"), LabelText("数值参数")]
        public string NumericValue;
    }
}