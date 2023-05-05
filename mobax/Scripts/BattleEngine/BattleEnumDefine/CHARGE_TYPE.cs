using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("冲锋类型")]
    public enum CHARGE_TYPE
    {
        [LabelText("冲锋固定时间")]
        Time,
        [LabelText("冲锋到目标")]
        Destation
    }
}