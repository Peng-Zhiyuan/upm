using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotMoveActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.MoveAbsolute;
        public int ChooseId = 0;
        public bool OpenPos = false;
        public Vector3 EndPos = Vector3.zero;
        public bool OpenPosCurve;
        public AnimationCurve PosCurve;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsMoveActionElement2 actionElement = (PlotComicsMoveActionElement2)element;
            this.ChooseId = actionElement.chooseId;
            this.OpenPos = actionElement.openPos;
            this.EndPos = actionElement.endPos;
            this.OpenPosCurve = actionElement.openPosCurve;
            this.PosCurve = actionElement.posCurve;
        }
    }

    public class PlotMoveActionTask : PlotActionAbilityTask
    {
        public PlotMoveActionTaskData TaskData => (PlotMoveActionTaskData)this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);

            this.InitSomeRoot();
            this.InitPosCurve();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);

            if (this.TaskData.OpenPos)
            {
                this.UpdatePosition(frameIdx);
            }
        }

        public override  async Task EndExecute()
        {
            base.EndExecute();

            // 还原层级
            // this.ReplaceLayer();
        }

        #region ---初始化---

        private GameObject _roleObj;
        private Vector3 _startPos;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.ChooseId);
            this._roleObj = cacheInfo.ModelObj;
            this._startPos = cacheInfo.Pos;

            this.Move();
        }

        private void Move()
        {
            var animator = this._roleObj.GetComponentInChildren<Animator>();
            animator.SetActive(true);
            if (animator != null)
            {
                animator.Play("run");
                Debug.Log("[Plot]--->开始播放run动作");
            }
        }

        #region ---更新---

        private int _lastPositionFrame;

        public void UpdatePosition(int frameIdx)
        {
            if (this._lastPositionFrame == frameIdx) return;
            int startFrame = this.TaskData.StartFrame;

            if (frameIdx - startFrame < this._positionCurveData.Count)
            {
                Vector3 targetPosition = this._startPos + this._positionCurveData[frameIdx - startFrame];
                var targetDir = targetPosition - this._roleObj.transform.localPosition;
                var angle = Vector3.Angle(targetDir, Vector3.forward);
                this._roleObj.transform.localPosition = targetPosition;
                this._roleObj.transform.localEulerAngles = new Vector3(0, angle, 0);
            }

            this._lastPositionFrame = frameIdx;
        }

        #endregion

        #endregion

        #region ---曲线相关---

        private List<Vector3> _positionCurveData = new List<Vector3>();

        private void InitPosCurve()
        {
            if (!this.TaskData.OpenPos) return;
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            var sp = this._startPos;

            for (int i = 0; i <= frameOffset; i++)
            {
                float curveSpeed = this.TaskData.OpenPosCurve
                    ? this.TaskData.PosCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;

                float x = (this.TaskData.EndPos.x - sp.x) * curveSpeed;
                float y = (this.TaskData.EndPos.y - sp.y) * curveSpeed;
                float z = (this.TaskData.EndPos.z - sp.z) * curveSpeed;
                this._positionCurveData.Add(new Vector3(x, y, z));
            }
        }

        #endregion
    }
}