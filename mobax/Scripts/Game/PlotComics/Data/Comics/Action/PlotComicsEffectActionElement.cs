using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("特效", (int) EPlotActionType.Effect)]
    public class PlotComicsEffectActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "特效";

        [FilePath(ParentFolder = "Assets/Arts/FX")] [ToggleGroup("enabled"), LabelText("特效路径")]
        public string effectRes = "";

        [LabelWidth(80)] [ToggleGroup("enabled"), LabelText(" 缩放")]
        public Vector3 scale = Vector3.one;
        
        [ToggleGroup("enabled"), LabelText("速度改变")]
        public Dictionary<int, float> speedModify = new Dictionary<int, float>();

        [ToggleGroup("enabled"), LabelText("坐标类型")]
        public EPlotEffectCoordinateType attachType = EPlotEffectCoordinateType.Global;

        public bool NeedAttachPoint
        {
            get { return attachType == EPlotEffectCoordinateType.Target; }
        }

        public bool NeedOffset
        {
            get
            {
                return attachType == EPlotEffectCoordinateType.Global || attachType == EPlotEffectCoordinateType.Target;
            }
        }

        [LabelWidth(80)] [ToggleGroup("enabled"), LabelText("绑点"), ShowIf("NeedAttachPoint")]
        public string attachPoint = "";

        [LabelWidth(80)] [ToggleGroup("enabled"), LabelText("绑点目标ID")]
        public int attachTargetId = 0;

        [LabelWidth(80)] [ToggleGroup("enabled"), LabelText(" 位置偏移"), ShowIf("NeedOffset")]
        public Vector3 posOffset = Vector3.zero;

        [LabelWidth(80)] [ToggleGroup("enabled"), LabelText(" 角度偏移"), ShowIf("NeedOffset")]
        public Vector3 angleOffset = Vector3.zero;

        [LabelWidth(150)] [ToggleGroup("enabled"), LabelText("是否跟着角色移动")]
        public bool isAttachLock = false;

        [LabelWidth(150)]
        // [HorizontalGroup("attach", 0.5f, LabelWidth = 120)]
        [ToggleGroup("enabled"), LabelText("是否跟着角色旋转")]
        public bool isAttachLookAt = true;

        [LabelWidth(150)] [ToggleGroup("enabled"), LabelText("是否朝向目标位置")]
        public bool isLookAtTarget = false;

        [LabelWidth(150)] [ToggleGroup("enabled"), LabelText("朝向目标ID"), ShowIf("isLookAtTarget")]
        public int lookAtTargetId = 0;

        [LabelWidth(150)]
        //[HorizontalGroup("other", 0.5f, LabelWidth = 120)]
        [ToggleGroup("enabled"), LabelText("是否循环")]
        public bool isloop = false;
    }
}