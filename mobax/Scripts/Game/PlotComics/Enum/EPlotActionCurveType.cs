using Sirenix.OdinInspector;

namespace Plot.Runtime
{
    public enum EPlotActionCurveType
    {
        [LabelText("位移")] Position,
        [LabelText("旋转")] Rotation,
        [LabelText("缩放")] Scale,
        [LabelText("透明度")] Alpha,
    }
}