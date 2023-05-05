using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("盾墙类型")]
    public enum SHIELD_WALL_TYPE
    {
        [LabelText("圆形")]
        Cylinder = 0,
        [LabelText("矩形")]
        Cube = 1
    }
}