using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("参考图", (int) EPlotComicsElementType.ReferenceImage,
        (int) EConfigPriority.ReferenceImage)]
    public class PlotComicsReferenceImageElement : PlotComicsConfigElementItem
    {
        public override string Label => "参考图";

        [ToggleGroup("enabled"), LabelText("参考图资源")]
        [Tooltip("参考图")]
        [LabelWidth(65)]
        //TODO:这里最好不要用0.000多位小数点
        [HorizontalGroup("enabled/model", 0.664f)]
        [VerticalGroup("enabled/model/Setting")]
        [TitleGroup("enabled/model/Setting/Reference Base Setting")]
        [VerticalGroup("enabled/model/Setting/Reference Base Setting/Setting")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Editor/Plot/PlotEditor/ReferenceImages",
            Extensions = "png")]
        [OnValueChanged("InitMaskTexture")]
        public string referenceImage;

        // public string DynamicParent = "Assets/Arts/Plots/Plot2D/$plots_Images/$plots_image_mask";

        [VerticalGroup("enabled/model/Setting/Reference Base Setting/Setting")]
        [ToggleGroup("enabled")]
        [HideReferenceObjectPicker]
        [LabelWidth(100)]
        [LabelText(" Transform ")]
        public PlotComicsTransBaseSetting trans = new PlotComicsTransBaseSetting();

        [HorizontalGroup("enabled/model")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 140)]
        [ReadOnly]
        [VerticalGroup("enabled/model/rightInfo")]
        [HideLabel]
        public Texture2D referenceTexture;

        public void InitMaskTexture()
        {
            // 初始化frameName
            var split = this.referenceImage.Split(new char[] {'_'});
            var index = split.Last();

#if UNITY_EDITOR
            this.referenceTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Editor/Plot/PlotEditor/ReferenceImages/{this.referenceImage}");
#endif
        }
    }
}