using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [Serializable]
    public class PlotComicsActionCurve
    {
        [LabelText("改变属性:")] [LabelWidth(80)] public EPlotActionCurveType curveType;

        [HideIf("IsAlpha")] [LabelText("初始值:")] [LabelWidth(80)]
        public Vector3 sp;

        [HideIf("IsAlpha")] [LabelText("目标值:")] [LabelWidth(80)]
        public Vector3 ep;

        [LabelText("初始值:")] [LabelWidth(80)] [ShowIf("IsAlpha")]
        public float sa;

        [ShowIf("IsAlpha")] [LabelText("目标值:")] [LabelWidth(80)]
        public float ea;

        [LabelText("开启曲线:")] [LabelWidth(80)] public bool openCurve;

        [LabelText("")] [LabelWidth(40)] [ShowIf("openCurve")]
        public AnimationCurve curve;

        private bool IsAlpha()
        {
            return this.curveType.Equals(EPlotActionCurveType.Alpha);
        }
    }
}