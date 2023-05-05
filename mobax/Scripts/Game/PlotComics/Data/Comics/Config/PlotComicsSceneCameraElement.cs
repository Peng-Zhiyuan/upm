using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("场景相机", (int) EPlotComicsElementType.SceneCamera, (int) EConfigPriority.SceneCamera)]
    public class PlotComicsSceneCameraElement : PlotComicsConfigElementItem
    {
        #region ---抽象---

        public override string Label => "场景相机";

        #endregion

        [ToggleGroup("enabled")] [LabelText("相机资源:")] [LabelWidth(92)] [ValueDropdown("GetMainCameraNames")]
        public string cameraName = "(场景相机)";

        [ToggleGroup("enabled")] [LabelWidth(92)] [LabelText("锁定不再修改:")]
        public bool isLock = false;

        [ToggleGroup("enabled")] [LabelText(" 相机位置:")] [LabelWidth(80)]
        // [OnValueChanged("OnCameraChanged")]
        public Vector3 cameraPos;

        [ToggleGroup("enabled")] [LabelText(" 相机旋转:")] [LabelWidth(80)]
        // [OnValueChanged("OnCameraChanged")]
        public Vector3 cameraRotation;

        [ToggleGroup("enabled")]
        [TitleGroup("enabled/Blur Setting")]
        [LabelText("Open Radia Blur")]
        [LabelWidth(140)]
        [UnityEngine.Tooltip("场景模糊")]
        public bool openBlur = false;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Blur Setting/setting", VisibleIf = "openBlur")]
        [LabelWidth(140)]
        [LabelText("Radia Blur Center X")]
        [Range(0, 1)]
        public float blurCenterX = 0.5f;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Blur Setting/setting", VisibleIf = "openBlur")]
        [LabelWidth(140)]
        [LabelText("Radia Blur Center X")]
        [Range(0, 1)]
        public float blurCenterY = 0.5f;

        [ToggleGroup("enabled")]
        [Title("FOV:")]
        [HideLabel]
        [ProgressBar(0, 100, ColorMember = "GetProgressBarColor", DrawValueLabel = false)]
        public float fov = 60f;

        private Color GetProgressBarColor(int value)
        {
            return Color.Lerp(new Color(1, 0.92f, 0.016f), new Color(0.5f, 1, 0), Mathf.Pow(value / 100f, 2));
        }

        [ToggleGroup("enabled")]
        [TitleGroup("enabled/Physical Camera Setting")]
        [ToggleGroup("enabled")]
        [LabelText("Open Physical Camera")]
        [LabelWidth(140)]
        public bool openPhysical = false;

        [ToggleGroup("enabled")]
        [UnityEngine.Tooltip("传感器和摄像机镜头之间的距离，即焦距")]
        [VerticalGroup("enabled/Physical Camera Setting/setting", VisibleIf = "openPhysical")]
        [LabelWidth(140)]
        [LabelText("Focal Length")]
        public float focalLength = 50;

        [ToggleGroup("enabled")]
        [UnityEngine.Tooltip("捕捉图像的传感器的宽度和高度，表示传感器大小")]
        [VerticalGroup("enabled/Physical Camera Setting/setting")]
        [LabelWidth(140)]
        [LabelText(" Sensor Size")]
        public Vector2 sensorSize = new Vector2(36, 24);

        [ToggleGroup("enabled")]
        [UnityEngine.Tooltip("从传感器水平和垂直偏移摄像机的镜头")]
        [VerticalGroup("enabled/Physical Camera Setting/setting")]
        [LabelWidth(140)]
        [LabelText(" Lens Shift")]
        public Vector2 lensShift = Vector2.zero;

        [ToggleGroup("enabled")]
        [VerticalGroup("enabled/Physical Camera Setting/setting")]
        [LabelWidth(140)]
        [LabelText("Gate Fit")]
        public Camera.GateFitMode gateFit = Camera.GateFitMode.Horizontal;

        /// <summary>
        /// 获取场景内所有的目标组件
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetMainCameraNames()
        {
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            List<string> targetNames = new List<string>();
            targetNames.Insert(0, "(场景相机)");
            if (cameras.Length <= 0) return targetNames;

            for (int i = 0; i < cameras.Length; i++)
            {
                targetNames.Add(cameras[i].name);
            }

            return targetNames;
        }
    }
}