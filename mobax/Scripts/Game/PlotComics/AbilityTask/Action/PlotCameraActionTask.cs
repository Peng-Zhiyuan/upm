using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotCameraActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Camera;

        public Vector3 EndPos;
        public bool OpenPosCurve;
        public AnimationCurve PosCurve;
        public Vector3 EndRotation;
        public bool OpenRotationCurve;
        public AnimationCurve RotationCurve;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsCameraActionElement actionElement = (PlotComicsCameraActionElement) element;
            this.EndPos = actionElement.endPos;
            this.OpenPosCurve = actionElement.openPosCurve;
            this.PosCurve = actionElement.posCurve;
            this.EndRotation = actionElement.endRotation;
            this.OpenRotationCurve = actionElement.openRotationCurve;
            this.RotationCurve = actionElement.rotationCurve;
        }
    }

    public class PlotCameraActionTask : PlotActionAbilityTask
    {
        public PlotCameraActionTaskData TaskData => (PlotCameraActionTaskData) this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            this.InitSomeRoot();
            await this.InitAnimCurve(true);
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            this.UpdatePosition(frameIdx);
            this.UpdateRotation(frameIdx);
        }

        public override  async Task EndExecute()
        {
            base.EndExecute();
        }

        #region ---更新---

        private int _lastPositionFrame;
        private int _lastRotationFrame;

        public void UpdatePosition(int frameIdx)
        {
            if (this._lastPositionFrame == frameIdx) return;
            int startFrame = this.TaskData.StartFrame;

            if (frameIdx - startFrame < this._positionCurveData.Count)
            {
                Vector3 targetPosition = this._startPos + this._positionCurveData[frameIdx - startFrame];
                this._camera.transform.localPosition = targetPosition;
            }

            this._lastPositionFrame = frameIdx;
        }

        public void UpdateRotation(int frameIdx)
        {
            if (this._lastRotationFrame == frameIdx) return;

            int startFrame = this.TaskData.StartFrame;
            if (frameIdx - startFrame < this._rotationCurveData.Count)
            {
                Vector3 targetRotation = this._startRotation + this._rotationCurveData[frameIdx - startFrame];
                this._camera.transform.localEulerAngles = targetRotation;
            }

            this._lastRotationFrame = frameIdx;
        }

        #endregion

        #region ---初始化---

        private Vector3 _startPos;
        private Vector3 _startRotation;

        private GameObject _cameraRoot;
        private Camera _camera;
        private static readonly int ShaderMask = Shader.PropertyToID("_Mask");

        private void InitSomeRoot()
        {
            this._cameraRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_CAMERA_PATH).gameObject;
            this._camera = this._cameraRoot.GetComponent<Camera>();
            this._startPos = this._camera.transform.localPosition;
            this._startRotation = this._camera.transform.localEulerAngles;
        }

        #endregion

        #region ---曲线相关---

        private List<Vector3> _positionCurveData = new List<Vector3>();
        private List<Vector3> _rotationCurveData = new List<Vector3>();

        private async Task InitAnimCurve(bool isInit = false)
        {
            this._positionCurveData = new List<Vector3>();
            this._rotationCurveData = new List<Vector3>();

            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            for (int i = 0; i <= frameOffset; i++)
            {
                float curvePosSpeed = this.TaskData.OpenPosCurve
                    ? this.TaskData.PosCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;
                float x1 = (this.TaskData.EndPos.x - this._startPos.x) * curvePosSpeed;
                float y1 = (this.TaskData.EndPos.y - this._startPos.y) * curvePosSpeed;
                float z1 = (this.TaskData.EndPos.z - this._startPos.z) * curvePosSpeed;
                this._positionCurveData.Add(new Vector3(x1, y1, z1));


                float curveRotationSpeed = this.TaskData.OpenRotationCurve
                    ? this.TaskData.RotationCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;
                float x2 = (this.TaskData.EndRotation.x - this._startRotation.x) * curveRotationSpeed;
                float y2 = (this.TaskData.EndRotation.y - this._startRotation.y) * curveRotationSpeed;
                float z2 = (this.TaskData.EndRotation.z - this._startRotation.z) * curveRotationSpeed;
                this._rotationCurveData.Add(new Vector3(x2, y2, z2));
            }
        }

        #endregion
    }
}