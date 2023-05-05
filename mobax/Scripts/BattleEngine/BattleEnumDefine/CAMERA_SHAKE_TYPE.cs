using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("相机抖动类型")]
    public enum CAMERA_SHAKE_TYPE
    {
        [LabelText("XZ 轴")]
        AixOffsetXZ,
        [LabelText("XY轴")]
        CameraXY,
        [LabelText("缩放")]
        Zoom,
        [LabelText("旋转")]
        Rotate,
        [LabelText("Y轴")]
        AixOffsetY
    }
}