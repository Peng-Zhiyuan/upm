using System.Security.Policy;
using UnityEngine;

namespace BattleEngine.Logic
{
    public sealed class VimPoint
    {
        public int Value { get; private set; }
        public int MaxValue { get; private set; }
        
        public void Reset()
        {
            Value = MaxValue;
        }

        public void SetMaxValue(int value)
        {
            MaxValue = value;
        }
        
        public void Minus(int value)
        {
            Value = Mathf.Max(0, Value - value);
        }

        public void Add(int value)
        {
            Value = Mathf.Min(MaxValue, Value + value);
        }

        public float Percent()
        {
            return (float)Value / MaxValue;
        }

        public int PercentHealth(int pct)
        {
            return (int)(MaxValue * (pct * 0.01f));
        }
    }
}