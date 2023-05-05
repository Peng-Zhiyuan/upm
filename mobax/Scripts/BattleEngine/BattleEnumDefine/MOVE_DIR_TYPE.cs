using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("移动方向")]
    public enum MOVE_DIR_TYPE
    {
        [LabelText("向前")]
        Froward,
        [LabelText("向后")]
        Back,
        [LabelText("向左")]
        Left,
        [LabelText("向右")]
        Right,
    }

    [System.Serializable]
    [LabelText("移动方式")]
    public enum MOVE_MODE_TYPE
    {
        [LabelText("瞬移")]
        Immediate,
        [LabelText("速度位移")]
        Speed,
        [LabelText("目标位置")]
        TargetPos
    }
}