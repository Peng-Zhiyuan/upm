using Sirenix.OdinInspector;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("屏幕震动", (int) EPlotActionType.ScreenShark)]
    public class PlotComicsScreenSharkActionElement : PlotComicsActionElementItem
    {
        public override string Label => "屏幕震动";

        [ToggleGroup("Enabled"), LabelText("震屏强度"), LabelWidth(100)]
        public float shakePower;

        [ToggleGroup("Enabled"), LabelText("震屏时长"), LabelWidth(100)]
        public float shakeTime;
    }
}