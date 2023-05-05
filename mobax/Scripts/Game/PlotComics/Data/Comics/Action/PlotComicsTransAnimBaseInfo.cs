using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotComicsTransAnimBaseInfo
    {
        [TitleGroup("Transform Position Change Setting", "Change Cur Pos ---> Target Value",
            VisibleIf = "@true")]
        [HideLabel]
        [Tooltip("开启位移设置")]
        [LabelText("Open Setting")]
        [LabelWidth(120)]
        public bool openPos = false;


        [VerticalGroup("Transform Position Change Setting/setting", VisibleIf = "openPos")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(120)]
        public Vector3 endPos = Vector3.zero;


        [VerticalGroup("Transform Position Change Setting/setting")]
        [LabelText("Open Curve")]
        [LabelWidth(120)]
        [Tooltip("开启动画曲线")]
        public bool openPosCurve;


        [VerticalGroup("Transform Position Change Setting/setting")]
        [LabelText("Pos Curve")]
        [LabelWidth(120)]
        [ShowIf("openPosCurve")]
        [HideReferenceObjectPicker]
        public AnimationCurve posCurve = new AnimationCurve();


        [TitleGroup("Transform Rotation Change Setting", "Change Cur Rotation ---> Target Value",
            VisibleIf = "@true")]
        [HideLabel]
        [Tooltip("开启旋转设置")]
        [LabelText("Open Setting")]
        [LabelWidth(120)]
        public bool openRotation = false;


        [VerticalGroup("Transform Rotation Change Setting/setting", VisibleIf = "openRotation")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(120)]
        public Vector3 endRotation = Vector3.zero;


        [VerticalGroup("Transform Rotation Change Setting/setting")]
        [LabelText("Open Curve")]
        [LabelWidth(120)]
        [Tooltip("开启动画曲线")]
        public bool openRotationCurve;


        [VerticalGroup("Transform Rotation Change Setting/setting")]
        [LabelText("Rotation Curve")]
        [LabelWidth(120)]
        [ShowIf("openRotationCurve")]
        [HideReferenceObjectPicker]
        public AnimationCurve rotationCurve = new AnimationCurve();


        [TitleGroup("Transform Scale Change Setting", "Change Cur Scale ---> Target Value",
            VisibleIf = "@true")]
        [HideLabel]
        [Tooltip("开启旋转设置")]
        [LabelText("Open Setting")]
        [LabelWidth(120)]
        public bool openScale = false;

        [VerticalGroup("Transform Scale Change Setting/setting", VisibleIf = "openScale")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(120)]
        public Vector3 endScale = Vector3.zero;

        [VerticalGroup("Transform Scale Change Setting/setting")]
        [LabelText("Open Curve")]
        [LabelWidth(120)]
        [Tooltip("开启动画曲线")]
        public bool openScaleCurve;


        [VerticalGroup("Transform Scale Change Setting/setting")]
        [LabelText("Scale Curve")]
        [LabelWidth(120)]
        [ShowIf("openScaleCurve")]
        [HideReferenceObjectPicker]
        public AnimationCurve scaleCurve = new AnimationCurve();


        [TitleGroup("Alpha Change Setting", "Change Cur Alpha ---> Target Value", VisibleIf = "@true")]
        [HideLabel]
        [Tooltip("开启透明度设置")]
        [LabelText("Open Setting")]
        [LabelWidth(120)]
        public bool openAlpha = false;


        [VerticalGroup("Alpha Change Setting/setting", VisibleIf = "openAlpha")]
        [LabelText(" Target Value ")]
        [Tooltip("透明度目标值")]
        [LabelWidth(120)]
        [Range(0, 1)]
        public float endAlpha = 0;

        [VerticalGroup("Alpha Change Setting/setting")] [LabelText("Open Curve")] [LabelWidth(120)] [Tooltip("开启动画曲线")]
        public bool openAlphaCurve;

        [VerticalGroup("Alpha Change Setting/setting")]
        [LabelText("Alpha Curve")]
        [LabelWidth(120)]
        [ShowIf("openAlphaCurve")]
        [HideReferenceObjectPicker]
        public AnimationCurve alphaCurve = new AnimationCurve();
    }
}