using Coffee.UIEffects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("遮罩框溶解入场", (int) EPlotActionType.MaskDissolve)]
    public class PlotComicsMaskDissolveActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "遮罩框溶解入场";

        [ToggleGroup("enabled"), LabelText("遮罩资源:")]
        [FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$Images/Mask")]
        [LabelWidth(100)]
        public string maskRes;

        [ToggleGroup("enabled")] [TitleGroup("enabled/Dissolve Setting","Only Playing Mode")] [HideLabel] [ReadOnly]
        public UITransitionEffect.EffectMode dissolveMode = UITransitionEffect.EffectMode.Dissolve;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Dissolve Setting/setting")]
        [LabelText("Dissolve Width")]
        [LabelWidth(160)]
        [Range(0, 1)]
        public float dissolveWidth = 0f;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Dissolve Setting/setting")]
        [LabelText("Dissolve Softness")]
        [LabelWidth(160)]
        [Range(0, 1)]
        public float dissolveSoftness = 0f;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Dissolve Setting/setting")]
        [LabelText("Dissolve Effect Dir")]
        [LabelWidth(160)]
        [Tooltip("目前支持左右两种")]
        public UITransitionEffect.EffectDir effectDir = UITransitionEffect.EffectDir.Right;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Dissolve Setting/setting")]
        [LabelWidth(160)]
        [LabelText("Dissolve Texture")]
        [FilePath(ParentFolder = "Assets/Arts/Plots/Plot2D/$DissolveImg")]
        public string textureRes;
    }
}