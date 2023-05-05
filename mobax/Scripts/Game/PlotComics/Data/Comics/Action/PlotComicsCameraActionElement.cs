using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("镜头动画", (int) EPlotActionType.Camera)]
    public class PlotComicsCameraActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "镜头动画";

        [ToggleGroup("enabled"), LabelText("Camera Typ")] [LabelWidth(100)]
        public EPlotCameraType cameraType = EPlotCameraType.MainCamera;

        [ToggleGroup("enabled"), LabelText("Is Back")] [LabelWidth(100)] [Tooltip("勾选后则重新播放时相机会优先重置为此值")]
        public bool isReset;
        
        [ToggleGroup("enabled")]
        [TitleGroup("enabled/Transform Position Change Setting", "Change Cur Pos ---> Target Value")] [HideLabel] [Tooltip("开启位移设置")]
        [LabelText("Open Setting")]
        [LabelWidth(100)]
        public bool openPos = false;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting", VisibleIf = "openPos")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(100)]
        public Vector3 endPos = Vector3.zero;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting")] [LabelText("Anim Curve")] [LabelWidth(100)]
        [Tooltip("开启动画曲线")]
        public bool openPosCurve;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Position Change Setting/setting")]
        [LabelText("")]
        [LabelWidth(46)]
        [ShowIf("openPosCurve")]
        public AnimationCurve posCurve;
        
        [ToggleGroup("enabled")]
        [TitleGroup("enabled/Transform Rotation Change Setting", "Change Cur Rotation ---> Target Value")] [HideLabel] [Tooltip("开启旋转设置")]
        [LabelText("Open Setting")]
        [LabelWidth(100)]
        public bool openRotation = false;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Rotation Change Setting/setting", VisibleIf = "openRotation")]
        [LabelText(" Target Value ")]
        [Tooltip("位移目标值")]
        [LabelWidth(100)]
        public Vector3 endRotation = Vector3.zero;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Rotation Change Setting/setting")] [LabelText("Anim Curve")] [LabelWidth(100)]
        [Tooltip("开启动画曲线")]
        public bool openRotationCurve;
        
        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Transform Rotation Change Setting/setting")]
        [LabelText("")]
        [LabelWidth(46)]
        [ShowIf("openRotationCurve")]
        public AnimationCurve rotationCurve;
    }
}