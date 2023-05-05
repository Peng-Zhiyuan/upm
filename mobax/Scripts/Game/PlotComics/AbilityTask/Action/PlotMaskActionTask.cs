using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Plot.Runtime
{
    public class PlotMaskActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Mask;
        public PlotComicsTransAnimBaseInfo TransAni;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsMaskActionElement actionElement = (PlotComicsMaskActionElement) element;
            this.TransAni = actionElement.transAni;
        }
    }

    public class PlotMaskActionTask : PlotActionAbilityTask
    {
        public PlotMaskActionTaskData TaskData => (PlotMaskActionTaskData) this.TaskInitData;


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
        }

        public override void ReExecute()
        {
            base.ReExecute();
            // if (!this._configData.OpenPicture) return;
            if (this._pictureObj != null)
            {
                var image = this._pictureObj.GetComponent<Image>();
                image.DOColor(new Color(200 / 255f, 200 / 255f, 200 / 255f, 255f / 255f), 0.8f);
                // .OnComplete(this.ReplaceLayer);
            }

            // this.ReplaceLayer();
        }

        public override async Task EndExecute()
        {
            base.EndExecute();

            // 还原层级
            // this.ReplaceLayer();
        }

        // 将气泡的层级挪到bubbleObj中去
        private void ReplaceLayer()
        {
            // 将框移动到下方
            var frameRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_FRAME_ROOT_PATH).gameObject;
            this._frameObj.transform.SetParent(frameRoot.transform);

            // 将气泡移动到上方
            var bubbleObj = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_BUBBLE_ROOT_PATH).gameObject;

            var temp = new List<GameObject>();
            for (int i = 0; i < this._frameObj.transform.childCount; i++)
            {
                var behaviour = this._frameObj.transform.GetComponentInChildren<PlotComicsBubbleBehaviour>();
                if (behaviour == null) continue;

                temp.Add(behaviour.gameObject);
            }

            if (temp.Count <= 0) return;

            foreach (var t in temp)
            {
                this.BubbleReFadeInAni(t, () => { t.transform.SetParent(bubbleObj.transform); });
            }
        }

        // 为了让气泡的出现更加平缓
        private void BubbleReFadeInAni(GameObject obj, Action action)
        {
            action?.Invoke();
            // var canvas = obj.GetComponent<CanvasGroup>();
            // canvas.alpha = 0;
            // DOTween.To(() => canvas.alpha, x => canvas.alpha = x, 1, 0f).OnComplete(() => { action?.Invoke(); });
        }

        #region ---初始化---

        private PlotMaskFrameConfigTaskData _configData;
        private Vector3 _startPos;
        private Vector3 _startRotation;
        private Vector3 _startScale;
        private float _startAlpha;
        private Bucket Bucket => BucketManager.Stuff.Plot;
        private static readonly int ShaderMask = Shader.PropertyToID("_Mask");

        private GameObject _frameObj;
        private GameObject _rawImg;
        private GameObject _pictureRoot;
        private GameObject _maskPictureObj;
        private GameObject _pictureObj;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeMaskCacheManager.MaskCacheInfo;
            if (cacheInfo == null) return;
            this._configData = cacheInfo.MaskConfigData;
            this._frameObj = cacheInfo.MaskFrameCacheObj;

            if (this._frameObj != null)
            {
                this._startPos = this._frameObj.transform.rectTransform().localPosition;
                this._startRotation = this._frameObj.transform.rectTransform().localEulerAngles;
                this._startScale = this._frameObj.transform.rectTransform().localScale;

                var canvas = this._frameObj.transform.GetComponent<CanvasGroup>();
                this._startAlpha = canvas.alpha;
            }

            this._rawImg = cacheInfo.RawImgCacheObj;
            this._maskPictureObj = cacheInfo.MaskPictureObj;
            this._pictureObj = cacheInfo.FragPictureCacheObj;

            this._pictureRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_PICTURE_ROOT_PATH)
                .gameObject;
        }

        public async Task SetPicture()
        {
            if (this._configData == null || this._configData.OpenPicture) return;
            var sprite = await this.Bucket.GetOrAquireSpriteAsync(this._configData.MaskRes);
            var obj = new GameObject
            {
                name = $"{this._configData.MaskRes.Replace(".png", "")}_picture"
            };
            obj.transform.SetParent(this._pictureRoot.transform);
            var image = obj.GetOrAddComponent<Image>();
            var temp = await this.Bucket.GetOrAquireAsync<Material>("PlotMaskMat.mat");
            image.material = new Material(temp);
            image.material.SetTexture(ShaderMask, sprite.texture);
            // obj.transform.rectTransform().sizeDelta = new Vector2(1080, 1920);
            // obj.transform.rectTransform().localPosition = Vector3.zero;
            obj.transform.rectTransform().localScale = Vector3.one;

            var rawImg = this._rawImg.GetComponent<RawImage>();
            this._rawImg.SetActive(false);
            image.sprite = PlotRuntimeUtil.TextureToSprite(PlotRuntimeUtil.TextureToTexture2D(rawImg.texture));
            image.SetNativeSize();
            obj.transform.rectTransform().localPosition = this.TaskData.TransAni.endPos;
            this._pictureObj = obj;

            image.DOColor(new Color(200 / 255f, 200 / 255f, 200 / 255f, 255f / 255f), 0.8f);
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
            int startFrame = this.TaskData.StartFrame;
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this.DoPosition(this.TaskData.TransAni.endPos);
            }

            if (frameIdx - startFrame < this._positionCurveData.Count)
            {
                Vector3 targetPosition = this._startPos + this._positionCurveData[frameIdx - startFrame];
                this.DoPosition(targetPosition);
            }

            this._lastPositionFrame = frameIdx;
        }

        private void DoPosition(Vector3 position)
        {
            if (this._frameObj != null)
            {
                this._frameObj.transform.rectTransform().localPosition = position;
            }

            if (this._configData != null && !this._configData.OpenPicture)
            {
                if (this._rawImg != null)
                {
                    this._rawImg.transform.rectTransform().localPosition = position;
                }
            }
        }

        public void UpdateRotation(int frameIdx)
        {
            if (this._lastRotationFrame == frameIdx) return;

            int startFrame = this.TaskData.StartFrame;
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this.DoRotation(this.TaskData.TransAni.endRotation);
            }

            if (frameIdx - startFrame < this._rotationCurveData.Count)
            {
                Vector3 targetRotation = this._startRotation + this._rotationCurveData[frameIdx - startFrame];
                this.DoRotation(targetRotation);
            }

            this._lastRotationFrame = frameIdx;
        }

        private void DoRotation(Vector3 rotation)
        {
            if (this._frameObj != null)
            {
                this._frameObj.transform.rectTransform().localEulerAngles = rotation;
            }

            if (this._configData != null && !this._configData.OpenPicture)
            {
                if (this._rawImg != null)
                {
                    this._rawImg.transform.rectTransform().localEulerAngles = rotation;
                }
            }
        }

        public void UpdateScale(int frameIdx)
        {
            if (this._lastScaleFrame == frameIdx) return;

            int startFrame = this.TaskData.StartFrame;
            if (this.TaskData.EndFrame <= frameIdx)
            {
                this.DoScale(this.TaskData.TransAni.endScale);
            }

            if (frameIdx - startFrame < this._scaleCurveData.Count)
            {
                Vector3 targetScale = this._startScale + this._scaleCurveData[frameIdx - startFrame];
                this.DoScale(targetScale);
            }

            this._lastScaleFrame = frameIdx;
        }

        private void DoScale(Vector3 scale)
        {
            if (this._frameObj != null)
            {
                this._frameObj.transform.rectTransform().localScale = scale;
            }

            if (this._configData != null && !this._configData.OpenPicture)
            {
                if (this._rawImg != null)
                {
                    this._rawImg.transform.rectTransform().localScale = scale;
                }
            }
        }

        public void UpdateAlpha(int frameIdx)
        {
            if (this._lastAlphaFrame == frameIdx) return;

            if (this.TaskData.EndFrame <= frameIdx)
            {
                this.DoAlpha(this.TaskData.TransAni.endAlpha);
            }

            int startFrame = this.TaskData.StartFrame;
            if (frameIdx - startFrame < this._alphaCurveData.Count)
            {
                float targetAlpha = this._startAlpha + this._alphaCurveData[frameIdx - startFrame];
                this.DoAlpha(targetAlpha);
            }

            this._lastAlphaFrame = frameIdx;
        }

        private void DoAlpha(float alpha)
        {
            if (this._frameObj != null)
            {
                var canvas1 = this._frameObj.GetComponent<CanvasGroup>();
                canvas1.alpha = alpha;
            }

            if (this._configData != null && !this._configData.OpenPicture)
            {
                if (this._rawImg != null)
                {
                    var canvas2 = this._rawImg.GetComponent<CanvasGroup>();
                    canvas2.alpha = alpha;
                }
            }
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
            var sp = this._startPos;

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
            var sp = this._startRotation;

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