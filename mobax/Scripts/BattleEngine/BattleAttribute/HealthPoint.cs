namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class HealthPoint
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

        public void AddHp(int value)
        {
            Value = Mathf.Clamp(Value + value, 0, MaxValue);
        }

        public float Percent()
        {
            return (float)Value / MaxValue;
        }

        public int PercentHealth(int pct)
        {
            return (int)(MaxValue * (pct * 0.01f));
        }

        public void SetValue(int val)
        {
            Value = Mathf.Clamp(val, 0, MaxValue);
        }
    }
}