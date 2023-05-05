namespace BattleEngine.Logic
{
    using System;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [Serializable]
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class EffectAttribute : Attribute
    {
        readonly string effectType;
        readonly int order;

        public EffectAttribute(string effectType, int order)
        {
            this.effectType = effectType;
            this.order = order;
        }

        public string EffectType
        {
            get { return effectType; }
        }
        public int Order
        {
            get { return order; }
        }
    }

    [Serializable]
    public abstract class Effect
    {
        [HideInInspector]
        public bool IsSkillEffect;
        [HideInInspector]
        public virtual string Label => "Effect";
        [ToggleGroup("Enabled", "$Label")]
        public bool Enabled;
        [ToggleGroup("Enabled"), ShowIf("IsSkillEffect", true), LabelWidth(100)]
        public LOCK_DOWN_TARGET_TYPE AddSkillEffectTargetType;
        [ToggleGroup("Enabled"), HideIf("IsSkillEffect", true), LabelText("触发概率"), LabelWidth(100)]
        public string TriggerProbability = "100%";
    }
}