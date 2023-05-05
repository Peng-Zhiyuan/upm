using System.Collections.Generic;
using UnityEngine;

namespace BattleEngine.Logic
{
    public sealed class MagicPoint
    {
        public int Value { get; private set; }
        public int MaxValue { get; private set; }

        public void Reset()
        {
            Value = 0;
        }

        public void SetMaxValue(int value)
        {
            MaxValue = value;
        }

        public void SetFull()
        {
            Value = MaxValue;
        }

        public void Minus(int value)
        {
            Value = Mathf.Max(0, Value - value);
        }

        public void Add(int value)
        {
            Value = Mathf.Max(0,Mathf.Min(MaxValue, Value + value)) ;
        }

        public float Percent()
        {
            return (float)Value / MaxValue;
        }

        public int PercentHealth(int pct)
        {
            return (int)(MaxValue * (pct / 100f));
        }
    }
}