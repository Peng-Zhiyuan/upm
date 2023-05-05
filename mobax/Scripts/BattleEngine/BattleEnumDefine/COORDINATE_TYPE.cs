using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("坐标类型"), LabelWidth(150)]
    public enum COORDINATE_TYPE
    {
        [LabelText("全局坐标")]
        Global = 0,
        [LabelText("本地坐标")]
        Local = 1,
        [LabelText("目标坐标")]
        Target = 2,
        [LabelText("预警坐标")]
        PreWarning = 3,
        [LabelText("输入坐标")]
        InputPoint = 4,
    }
}