namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    /// <summary>
    /// 浮点型修饰器
    /// </summary>
    public sealed class FloatModifier
    {
        public float Value;
        public bool Enabled = true;
    }

    /// <summary>
    /// 浮点型修饰器集合
    /// </summary>
    public sealed class FloatModifierCollection
    {
        public float TotalValue { get; private set; }
        private List<FloatModifier> Modifiers { get; } = new List<FloatModifier>();

        public float AddModifier(FloatModifier modifier)
        {
            Modifiers.Add(modifier);
            Update();
            return TotalValue;
        }

        public float RemoveModifier(FloatModifier modifier)
        {
            Modifiers.Remove(modifier);
            Update();
            return TotalValue;
        }

        public void Update()
        {
            TotalValue = 0;
            for (int i = 0; i < Modifiers.Count; i++)
            {
                TotalValue += Modifiers[i].Value;
            }
        }
    }

    /// <summary>
    /// 浮点型数值
    /// </summary>
    public sealed class FloatNumeric
    {
        public float Value { get; private set; }
        public float baseValue { get; private set; }
        // public float add { get; private set; }
        // public float pctAdd { get; private set; }
        public float finalAdd { get; private set; }
        public float finalPctAdd { get; private set; }
        // private FloatModifierCollection AddCollection { get; } = new FloatModifierCollection();
        // private FloatModifierCollection PctAddCollection { get; } = new FloatModifierCollection();
        private FloatModifierCollection FinalAddCollection { get; } = new FloatModifierCollection();
        private FloatModifierCollection FinalPctAddCollection { get; } = new FloatModifierCollection();

        public bool isPer = false;
        public bool fixData = false;

        public void Initialize()
        {
            baseValue = finalAdd = finalPctAdd = 0f;
        }

        public float SetBase(float value)
        {
            baseValue = value;
            Update();
            return baseValue;
        }

        // public void AddAddModifier(FloatModifier modifier) {
        //     add = AddCollection.AddModifier(modifier);
        //     Update();
        // }
        // public void AddPctAddModifier(FloatModifier modifier) {
        //     pctAdd = PctAddCollection.AddModifier(modifier);
        //     Update();
        // }
        public void AddFinalAddModifier(FloatModifier modifier)
        {
            finalAdd = FinalAddCollection.AddModifier(modifier);
            Update();
        }

        public void AddFinalPctAddModifier(FloatModifier modifier)
        {
            finalPctAdd = FinalPctAddCollection.AddModifier(modifier);
            Update();
        }

        // public void RemoveAddModifier(FloatModifier modifier) {
        //     add = AddCollection.RemoveModifier(modifier);
        //     Update();
        // }
        // public void RemovePctAddModifier(FloatModifier modifier) {
        //     pctAdd = PctAddCollection.RemoveModifier(modifier);
        //     Update();
        // }
        public void RemoveFinalAddModifier(FloatModifier modifier)
        {
            finalAdd = FinalAddCollection.RemoveModifier(modifier);
            Update();
        }

        public void RemoveFinalPctAddModifier(FloatModifier modifier)
        {
            finalPctAdd = FinalPctAddCollection.RemoveModifier(modifier);
            Update();
        }

        public void SetFixData(float fixValue)
        {
            fixData = true;
            Value = fixValue;
        }

        public void RemoveFixData(float value)
        {
            fixData = false;
            Update();
        }

        public void Update()
        {
            if (fixData)
            {
                return;
            }
            if (!isPer)
                Value = (baseValue + finalAdd) * (100 + finalPctAdd) * 0.01f;
            else
                Value = (baseValue + finalAdd) * (100 + finalPctAdd) * 0.01f * 0.01f;
        }
    }
}