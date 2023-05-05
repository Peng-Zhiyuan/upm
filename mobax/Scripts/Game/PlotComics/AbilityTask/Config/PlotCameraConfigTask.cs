using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotCameraConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.SceneCamera;
        public Vector3 Pos;
        public Vector3 Rotation;
        public float Fov = 60f;
        public bool OpenPhysical = false;
        public float FocalLength = 50;
        public Vector2 SensorSize = new Vector2(36, 24);
        public Vector2 LensShift = Vector2.zero;
        public Camera.GateFitMode GateFit = Camera.GateFitMode.Horizontal;
        public bool OpenBlur = false;
        public float BlurCenterX = 0.5f;
        public float BlurCenterY = 0.5f;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);

            PlotComicsSceneCameraElement configElement = (PlotComicsSceneCameraElement) element;
            this.Pos = configElement.cameraPos;
            this.Rotation = configElement.cameraRotation;
            this.Fov = configElement.fov;
            this.OpenPhysical = configElement.openPhysical;
            this.FocalLength = configElement.focalLength;
            this.SensorSize = configElement.sensorSize;
            this.LensShift = configElement.lensShift;
            this.GateFit = configElement.gateFit;
        }
    }

    public class PlotCameraConfigTask : PlotConfigAbilityTask
    {
        public PlotCameraConfigTaskData TaskData => (PlotCameraConfigTaskData) this.TaskInitData;

        public override async Task BeginExecute()
        {
            await base.BeginExecute();

            this.InitSomeRoot();
            this.InitCameraTrans();
            this.InitPhysicalCamera();
            this.InitBlur();
        }

        public override  async Task EndExecute()
        {
            base.EndExecute();
        }

        #region ---初始化---

        private GameObject _cameraRoot;
        private Camera _camera;

        void InitSomeRoot()
        {
            this._cameraRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_CAMERA_PATH).gameObject;
            this._camera = this._cameraRoot.GetComponent<Camera>();
        }

        private void InitCameraTrans()
        {
            this._cameraRoot.transform.localPosition = this.TaskData.Pos + PlotDefineUtil.ADD_SCENE_OFFSET;

            var rotation = this.TaskData.Rotation;
            this._cameraRoot.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        }

        private void InitPhysicalCamera()
        {
            this._camera.usePhysicalProperties = this.TaskData.OpenPhysical;
            if (this.TaskData.OpenPhysical)
            {
                this._camera.focalLength = this.TaskData.FocalLength;
                this._camera.sensorSize = this.TaskData.SensorSize;
                this._camera.lensShift = this.TaskData.LensShift;
                this._camera.gateFit = this.TaskData.GateFit;
            }

            this._camera.fieldOfView = this.TaskData.Fov <= 0 ? 60f : this.TaskData.Fov;
        }

        private void InitBlur()
        {
            var data = this._camera.GetComponent<CameraCustomData>();
            if (data == null) return;

            data.RadiaBlur.openBlur = this.TaskData.OpenBlur;
            if (this.TaskData.OpenBlur)
            {
                data.RadiaBlur.radiaBlurCenterX = this.TaskData.BlurCenterX;
                data.RadiaBlur.radiaBlurCenterY = this.TaskData.BlurCenterY;
            }
        }

        #endregion
    }
}