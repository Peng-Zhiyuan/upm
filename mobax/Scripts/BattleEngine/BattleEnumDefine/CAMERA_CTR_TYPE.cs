using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("相机控制类型")]
    public enum CAMERA_CTR_TYPE
    {
        [LabelText("抖动")]
        Shake = 0,
        [LabelText("聚焦")]
        Focus = 1,
        [LabelText("旋转")]
        Rotate = 12
    }
}