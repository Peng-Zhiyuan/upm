using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("气泡动画", (int) EPlotActionType.Bubble)]
    public class PlotComicsBubbleActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "气泡动画";

        #region ---NEW Data---

        [ToggleGroup("enabled")] [LabelWidth(100)] [LabelText("Choose Id")]
        public int chooseId;

        [ToggleGroup("enabled")] [LabelText("Preview Mode")] [Tooltip("开启预览模式，则场景拖动会刷新视图")] [LabelWidth(100)]
        public bool openPreviewMode = false;

        [ToggleGroup("enabled")]
        [Title("Transform Anim Setting")]
        [LabelText("Transform Properties")]
        [LabelWidth(200)]
        [HideReferenceObjectPicker]
        public PlotComicsTransAnimBaseInfo transAni = new PlotComicsTransAnimBaseInfo();

        #endregion

        #region ---OLD Data---

        // [ToggleGroup("enabled"), LabelText("气泡资源:")]
        // [FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$Images/Bubble")]
        // [LabelWidth(100)]
        // public string bubbleRes;
        //
        // [ToggleGroup("enabled"), LabelText("气泡唯一ID:")] [LabelWidth(100)]
        // public int bubbleId;
        //
        // [ToggleGroup("enabled"), LabelText("气泡对话:")]
        // [LabelWidth(100)]
        // [ListDrawerSettings(NumberOfItemsPerPage = 2)]
        // [PropertyOrder(1)]
        // public List<PlotComicBubbleWordArea> bubbleWords = new List<PlotComicBubbleWordArea>();
        //
        // [ToggleGroup("enabled"), LabelText("动画列表:")]
        // [LabelWidth(100)]
        // [PropertyOrder(2)]
        // [ListDrawerSettings(DraggableItems = false)]
        // public List<PlotComicsActionCurve> animCurves = new List<PlotComicsActionCurve>();

        #endregion
    }
}