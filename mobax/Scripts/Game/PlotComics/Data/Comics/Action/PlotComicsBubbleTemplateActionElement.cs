using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("气泡动画(固定模板放大->停留->变小)", (int) EPlotActionType.BubbleTemplate)]
    public class PlotComicsBubbleTemplateActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "气泡动画(固定模板放大->停留->变小)";

        [ToggleGroup("enabled"), LabelText("Choose Id")] [LabelWidth(100)]
        public int chooseId;

        [ToggleGroup("enabled"), LabelText("动画列表:")]
        [LabelWidth(100)]
        [PropertyOrder(2)]
        [ListDrawerSettings(DraggableItems = false)]
        public List<PlotComicsActionCurve> animCurves = new List<PlotComicsActionCurve>();
    }
}