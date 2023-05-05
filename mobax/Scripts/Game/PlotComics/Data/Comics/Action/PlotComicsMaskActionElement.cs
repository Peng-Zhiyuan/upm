using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("遮罩框动画", (int) EPlotActionType.Mask)]
    public class PlotComicsMaskActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "遮罩框动画";

        #region ---Old Style---

        // [ToggleGroup("enabled"), LabelText("遮罩资源:")]
        // [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/EditorTool/PlotComics/Res/$Images/Mask")]
        // [LabelWidth(100)]
        // [OnValueChanged("InitMaskTexture")]
        // public string referenceImage;
        //
        // [LabelWidth(100)] [ToggleGroup("enabled"), LabelText("边框资源: ")] [ReadOnly]
        // public string frameRes;
        //
        // [ToggleGroup("enabled"), LabelText("动画列表:")]
        // [LabelWidth(100)]
        // [PropertyOrder(2)]
        // [ListDrawerSettings(DraggableItems = false)]
        // public List<PlotComicsActionCurve> animCurves = new List<PlotComicsActionCurve>();

        // private void InitMaskTexture()
        // {
        //     // 初始化frameName
        //     var split = this.referenceImage.Split(new char[] { '_' });
        //     var index = split.Last();
        //     this.frameRes = $"frame_style_{index}";
        // }

        #endregion

        #region ---NEW Style---

        [ToggleGroup("enabled")] [LabelText("Preview Mode")] [Tooltip("开启预览模式，则场景拖动会刷新视图")] [LabelWidth(100)]
        public bool openPreviewMode = false;

        [ToggleGroup("enabled")]
        [Title("Transform Anim Setting")]
        [LabelText("Transform Properties")]
        [LabelWidth(200)]
        [HideReferenceObjectPicker]
        public PlotComicsTransAnimBaseInfo transAni = new PlotComicsTransAnimBaseInfo();

        #endregion
    }
}