using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("TimeLine", (int) EPlotComicsElementType.Timeline, (int) EConfigPriority.Timeline)]
    public class PlotComicsTimeLineConfigElement : PlotComicsConfigElementItem
    {
        public virtual string Label => "TimeLine";

        [FilePath(ParentFolder = "Assets/Arts/Plots/$Plot3D/plots_timelines", Extensions = "prefab")]
        [ToggleGroup("enabled")]
        [LabelWidth(80)]
        [LabelText("Res Path")]
        public string timeLinePath;
        // private string DynamicParent = PlotDefineUtil.PLOT_TIMELINE_PARENT_FOLDER;

        [ToggleGroup("enabled")] [LabelWidth(80)] [LabelText("ID")]
        public int timelineId;

        [ToggleGroup("enabled"), LabelText(" 模型位置:")] [LabelWidth(80)]
        public Vector3 pos;

        [ToggleGroup("enabled"), LabelText(" 模型旋转:")] [LabelWidth(80)]
        public Vector3 rotation;

        [ToggleGroup("enabled"), LabelText(" 模型缩放:")] [LabelWidth(80)]
        public Vector3 scale = Vector3.one;
    }
}