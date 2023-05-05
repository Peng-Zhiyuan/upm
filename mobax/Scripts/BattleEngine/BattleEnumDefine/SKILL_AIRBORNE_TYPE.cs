using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("击飞类型")]
    public enum SKILL_AIRBORNE_TYPE
    {
        [LabelText("自由")]
        FREE = 0,
        [LabelText("滞空")]
        HOVER = 1
    }

    [System.Serializable]
    [LabelText("吸星类型")]
    public enum SKILL_TOGETHER_TYPE
    {
        [LabelText("自由(平面)")]
        FREE = 0,
        [LabelText("滞空")]
        HOVER = 1
    }
}