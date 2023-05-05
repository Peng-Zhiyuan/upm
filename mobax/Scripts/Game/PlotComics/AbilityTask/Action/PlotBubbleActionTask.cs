using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Plot.Runtime
{
    public class PlotBubbleActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Bubble;
        public int ChooseId;
        public PlotComicsTransAnimBaseInfo TransAni;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsBubbleActionElement actionElement = (PlotComicsBubbleActionElement) element;
            this.ChooseId = actionElement.chooseId;
            this.TransAni = actionElement.transAni;
        }
    }

    public class PlotBubbleActionTask : PlotActionAbilityTask
    {
        public PlotBubbleActionTaskData TaskData => (PlotBubbleActionTaskData) this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);

            this.InitSomeRoot();
            this.InitCurve();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);

            if (this.TaskData.TransAni == null) return;
            if (this.TaskData.TransAni.openPos)
            {
                this.UpdatePosition(frameIdx);
            }

            if (this.TaskData.TransAni.openRotation)
            {
                this.UpdateRotation(frameIdx);
            }

            if (this.TaskData.TransAni.openScale)
            {
                this.UpdateScale(frameIdx);
            }

            if (this.TaskData.TransAni.openAlpha)
            {
                this.UpdateAlpha(frameIdx);
            }

            this.PlayWWise();
        }

        bool _isPlaying = false;

        void PlayWWise()
        {
            if (_isPlaying) return;
            var needPlay = false;
            if (this.TaskData.TransAni.openPos)
            {
                var data = this._positionCurveData;
                if (data != null && data.Count > 0)
                {
                    var first = data.First();
                    var last = data.Last();
                    if (Mathf.Abs(first.x) > Mathf.Abs(last.x) || Mathf.Abs(first.y) > Mathf.Abs(last.y))
                    {
                        needPlay = true;
                    }
                }
            }

            if (this.TaskData.TransAni.openScale)
            {
                var data = this._scaleCurveData;
                if (data != null && data.Count > 0)
                {
                    var first = data.First();
                    var last = data.Last();
                    if (!(Mathf.Abs(first.x) > Mathf.Abs(last.x)) && !(Mathf.Abs(first.y) > Mathf.Abs(last.y)))
                    {
                        needPlay = true;
                    }
                }
            }

            if (this.TaskData.TransAni.openAlpha)
            {
                var data = this._alphaCurveData;
                if (data != null && data.Count > 0)
                {
                    var first = data.First();
                    var last = data.Last();
                    if (!(Mathf.Abs(first) > Mathf.Abs(last)))
                    {
                        needPlay = true;
                    }
                }
            }

            if (needPlay)
            {
                _isPlaying = true;
                var comicsRes = PlotComicsManager.Stuff.CurPlayComicsRes;
                WwiseEventManager.SendEvent(TransformTable.Comics, $"{comicsRes}_{this.TaskData.ChooseId}");
            }
        }

        public override async Task EndExecute()
        {
            base.EndExecute();
            _isPlaying = false;
            // 把气泡停在最后一帧
        }

        #region ---初始化---

        private PlotBubbleConfigTaskData _configData;
        private Vector3 _startPos;
        private Vector3 _startRotation;
        private Vector3 _startScale;
        private float _startAlpha;
        private GameObject _bubbleObj;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeBubbleCacheManager.GetBubbleObj(this.TaskData.ChooseId);
            if (cacheInfo == null) return;
            this._configData = cacheInfo.ConfigElement;
            this._bubbleObj = cacheInfo.BubbleObj;

            if (this._bubbleObj != null)
            {
                this._startPos = this._bubbleObj.transform.rectTransform().localPosition;
                this._startRotation = this._bubbleObj.transform.rectTransform().localEulerAngles;
                this._startScale = this._bubbleObj.transform.rectTransform().localScale;

                var canvas = this._bubbleObj.transform.GetComponent<CanvasGroup>();
                if (canvas != null)
                {
                    this._startAlpha = canvas.alpha;
                }
            }
        }

        // 跳过的时候直接设置alpha为1
        public void BeginSkip(float alpha)
        {
            if (this._bubbleObj == null) return;

            var canvas = this._bubbleObj.transform.GetComponent<CanvasGroup>();
            if (canvas != null)
            {
                canvas.alpha = alpha;
            }
        }

        #endregion

        #region ---更新---

        private int _lastPositionFrame;
        private int _lastRotationFrame;
        private int _lastScaleFrame;
        private int _lastAlphaFrame;

        public void UpdatePosition(int frameIdx)
        {
            if (this._lastPositionFrame == frameIdx) return;
            if (this._bubbleObj == null) return;
            int startFrame = this.TaskData.StartFrame;
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this._bubbleObj.transform.rectTransform().localPosition = this.TaskData.TransAni.endPos;
            }

            if (frameIdx - startFrame < this._positionCurveData.Count)
            {
                Vector3 targetPosition = this._startPos + this._positionCurveData[frameIdx - startFrame];
                this._bubbleObj.transform.rectTransform().localPosition = targetPosition;
            }

            this._lastPositionFrame = frameIdx;
        }

        public void UpdateRotation(int frameIdx)
        {
            if (this._lastRotationFrame == frameIdx) return;

            if (this._bubbleObj == null) return;
            int startFrame = this.TaskData.StartFrame;
            // 为了添加矫正
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this._bubbleObj.transform.rectTransform().localEulerAngles = this.TaskData.TransAni.endRotation;
            }

            if (frameIdx - startFrame < this._rotationCurveData.Count)
            {
                Vector3 targetRotation = this._startRotation + this._rotationCurveData[frameIdx - startFrame];
                this._bubbleObj.transform.rectTransform().localEulerAngles = targetRotation;
            }

            this._lastRotationFrame = frameIdx;
        }

        public void UpdateScale(int frameIdx)
        {
            if (this._lastScaleFrame == frameIdx) return;

            if (this._bubbleObj == null) return;
            int startFrame = this.TaskData.StartFrame;
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this._bubbleObj.transform.rectTransform().localScale = this.TaskData.TransAni.endScale;
            }

            if (frameIdx - startFrame < this._scaleCurveData.Count)
            {
                Vector3 targetRotation = this._startScale + this._scaleCurveData[frameIdx - startFrame];
                this._bubbleObj.transform.rectTransform().localScale = targetRotation;
            }

            this._lastScaleFrame = frameIdx;
        }

        public void UpdateAlpha(int frameIdx)
        {
            if (this._lastAlphaFrame == frameIdx) return;

            int startFrame = this.TaskData.StartFrame;

            if (this._bubbleObj == null) return;
            var canvas1 = this._bubbleObj.GetComponent<CanvasGroup>();
            if (this.TaskData.EndFrame <= frameIdx)
            {
                canvas1.alpha = this.TaskData.TransAni.endAlpha;
            }

            if (frameIdx - startFrame < this._alphaCurveData.Count)
            {
                float targetAlpha = this._startAlpha + this._alphaCurveData[frameIdx - startFrame];
                canvas1.alpha = targetAlpha;
            }

            this._lastAlphaFrame = frameIdx;
        }

        #endregion

        #region ---曲线相关---

        // 气泡框只是UI部分
        private List<Vector3> _positionCurveData = new List<Vector3>();

        private List<Vector3> _rotationCurveData = new List<Vector3>();

        private List<Vector3> _scaleCurveData = new List<Vector3>();
        private List<float> _alphaCurveData = new List<float>();

        private void InitCurve()
        {
            this.InitPosCurve();
            this.InitRotationCurve();
            this.InitScaleCurve();
            this.InitAlphaCurve();
        }

        private void InitPosCurve()
        {
            if (!this.TaskData.TransAni.openPos) return;
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            var sp = _startPos;

            for (int i = 0; i <= frameOffset; i++)
            {
                float curveSpeed = this.TaskData.TransAni.openPosCurve
                    ? this.TaskData.TransAni.posCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;

                float x = (this.TaskData.TransAni.endPos.x - sp.x) * curveSpeed;
                float y = (this.TaskData.TransAni.endPos.y - sp.y) * curveSpeed;
                float z = (this.TaskData.TransAni.endPos.z - sp.z) * curveSpeed;
                this._positionCurveData.Add(new Vector3(x, y, z));
            }
        }

        private void InitRotationCurve()
        {
            if (!this.TaskData.TransAni.openRotation) return;
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            var sp = _startRotation;

            for (int i = 0; i <= frameOffset; i++)
            {
                float curveSpeed = this.TaskData.TransAni.openRotationCurve
                    ? this.TaskData.TransAni.rotationCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;

                float x = (this.TaskData.TransAni.endRotation.x - sp.x) * curveSpeed;
                float y = (this.TaskData.TransAni.endRotation.y - sp.y) * curveSpeed;
                float z = (this.TaskData.TransAni.endRotation.z - sp.z) * curveSpeed;
                this._rotationCurveData.Add(new Vector3(x, y, z));
            }
        }

        private void InitScaleCurve()
        {
            if (!this.TaskData.TransAni.openScale) return;
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            var sp = this._startScale;

            for (int i = 0; i <= frameOffset; i++)
            {
                float curveSpeed = this.TaskData.TransAni.openScaleCurve
                    ? this.TaskData.TransAni.scaleCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;

                float x = (this.TaskData.TransAni.endScale.x - sp.x) * curveSpeed;
                float y = (this.TaskData.TransAni.endScale.y - sp.y) * curveSpeed;
                float z = (this.TaskData.TransAni.endScale.z - sp.z) * curveSpeed;
                this._scaleCurveData.Add(new Vector3(x, y, z));
            }
        }

        private void InitAlphaCurve()
        {
            if (!this.TaskData.TransAni.openAlpha) return;
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            var sp = this._startAlpha;

            for (int i = 0; i <= frameOffset; i++)
            {
                float curveSpeed = this.TaskData.TransAni.openAlphaCurve
                    ? this.TaskData.TransAni.alphaCurve.Evaluate(i * 1f / frameOffset)
                    : i * 1f / frameOffset;

                float a = (this.TaskData.TransAni.endAlpha - sp) * curveSpeed;
                this._alphaCurveData.Add(a);
            }
        }

        #endregion
    }
}