using System.Linq;
using System.Threading.Tasks;
using ScreenFit;
using UnityEngine;
using UnityEngine.UI;

namespace Plot.Runtime
{
    public class PlotMaskFrameConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.CameraMask;
        public string MaskRes;
        public string FrameRes;
        public PlotComicsTransBaseSetting Trans;
        public bool OpenPicture;
        public string PictureRes;
        public Vector3 PictureStartPos;
        public bool OpenFit;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);
            PlotComicsMaskCameraElement configElement = (PlotComicsMaskCameraElement) element;
            this.MaskRes = configElement.maskRes;
            this.FrameRes = configElement.frameRes;
            this.Trans = configElement.trans;
            this.OpenPicture = configElement.openPicture;
            this.OpenFit = configElement.openFit;
            if (!string.IsNullOrEmpty(configElement.pictureRes))
            {
                var res = configElement.pictureRes.Split('/');
                this.PictureRes = res.Last();
            }
            else
            {
                this.PictureRes = "";
            }

            this.PictureStartPos = configElement.pictureStartPos;
        }
    }

    public class PlotMaskFrameConfigTask : PlotConfigAbilityTask
    {
        public PlotMaskFrameConfigTaskData TaskData => (PlotMaskFrameConfigTaskData) this.TaskInitData;

        public override async Task BeginExecute()
        {
            await base.BeginExecute();

            this.InitSomeRoot();
            await this.InitFrameTexture();
            // await this.InitMaskPictureTexture();
            await this.InitPicture();
            await this.InitRenderTexture();
            this.InitPos();
            this.ReplaceLayer();
            this.SaveCacheData();
        }

        public override async Task EndExecute()
        {
            // this.OnDestroy();
            base.EndExecute();
        }

        public override async Task Preload()
        {
            await base.Preload();
            if (!string.IsNullOrEmpty(this.TaskData.MaskRes))
            {
                await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.MaskRes);
            }

            if (!string.IsNullOrEmpty(this.TaskData.FrameRes))
            {
                await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.FrameRes);
            }

            if (!string.IsNullOrEmpty(this.TaskData.PictureRes))
            {
                await this.BucketPicture.GetOrAquireSpriteAsync(this.TaskData.PictureRes);
            }
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_MASK_FRAME_BUCKET);
        private Bucket BucketPicture => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_PICTURE_BUCKET);
        private GameObject _rawImg;
        private GameObject _frame;
        private GameObject _picture;
        private GameObject _dissolve;
        private Camera _camera;

        private GameObject _frameObj;
        private GameObject _pictureObj;
        private GameObject _maskPicture;
        private GameObject _maskPictureObj;
        private bool _haveTimeline;

        private static readonly int Clip = Shader.PropertyToID("_Clip");
        private static readonly int ShaderMask = Shader.PropertyToID("_Mask");

        private GameObject _frameChildObj;
        private Vector2Int _maskSizeDelta;

        private void InitSomeRoot()
        {
            this._rawImg = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_RAWIMG_ROOT_PATH).gameObject;
            // this._frame = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_FRONT_ROOT_PATH).gameObject;
            this._frame = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_FRAME_ROOT_PATH).gameObject;
            this._picture = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_PICTURE_ROOT_PATH).gameObject;
            this._maskPicture = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MASK_PICTURE_ROOT_PATH)
                .gameObject;
        }

        public void SetShowTimelineRawImg((bool showTimeline, int timelineId) timelineSetting)
        {
            this._haveTimeline = timelineSetting.showTimeline;
            if (!this._haveTimeline)
            {
                var cameraRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_CAMERA_PATH).gameObject;
                this._camera = cameraRoot.GetComponent<Camera>();
            }
            else
            {
                var cacheInfo = PlotRuntimeTimelineCacheManager.GetTimelineCache(timelineSetting.timelineId);
                if (cacheInfo == null)
                {
                    Debug.LogWarning("当前设置timeline相机出现未知错误！渲染顺序出错！");
                    return;
                }

                this._camera = cacheInfo.TimelineObj.transform.GetComponentInChildren<Camera>();
            }
        }

        private async Task InitRenderTexture()
        {
            var sprite = await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.MaskRes);

            var rawImg = this._rawImg.GetComponent<RawImage>();
            var material = rawImg.material;
            material.SetTexture(ShaderMask, sprite.texture);

            rawImg.rectTransform().sizeDelta = this._maskSizeDelta;
            LagacyUtil.SetRenderTexture(this._camera.transform, rawImg.transform, this._maskSizeDelta);

            if (!this.TaskData.OpenPicture)
            {
                this._rawImg.SetActive(!this.TaskData.OpenPicture);
                this._rawImg.transform.rectTransform().localPosition = this.TaskData.Trans.startPos;
                this._rawImg.transform.rectTransform().localEulerAngles = this.TaskData.Trans.startRotation;
                this._rawImg.transform.rectTransform().localScale = this.TaskData.Trans.startScale;
                var canvas2 = this._rawImg.GetOrAddComponent<CanvasGroup>();
                canvas2.alpha = this.TaskData.Trans.startAlpha;
            }
        }

        private async Task InitFrameTexture()
        {
            this._frameObj = new GameObject
            {
                name = $"{this.TaskData.FrameRes.Replace(".png", "")}_Root"
            };
            this._frameObj.transform.SetParent(this._frame.transform);
            // rectTf.localPosition = Vector3.zero;
            // rectTf.sizeDelta = new Vector2(1080, 1920);

            var canvas1 = this._frameObj.GetOrAddComponent<CanvasGroup>();
            canvas1.alpha = this.TaskData.Trans.startAlpha;

            var sprite = await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.FrameRes);
            var obj = new GameObject
            {
                name = $"{this.TaskData.FrameRes.Replace(".png", "")}"
            };
            var image = obj.GetOrAddComponent<Image>();
            // var canvas = obj.GetOrAddComponent<CanvasGroup>();
            // canvas.alpha = 1;

            image.sprite = sprite;
            image.SetNativeSize();

            var rectTf = this._frameObj.GetOrAddComponent<RectTransform>();
            rectTf.localPosition = this.TaskData.Trans.startPos;
            rectTf.sizeDelta = image.rectTransform().sizeDelta;

            obj.transform.SetParent(this._frameObj.transform);
            obj.transform.rectTransform().localPosition = Vector3.zero;

            if (!this._haveTimeline)
            {
                this._maskSizeDelta = new Vector2Int((int) image.rectTransform().sizeDelta.x,
                    (int) image.rectTransform().sizeDelta.y);
                // obj.GetOrAddComponent<CanvasFitter>();
            }
            else
            {
                this._maskSizeDelta = new Vector2Int(1080, 1920);
                var scaleFit = this._rawImg.GetOrAddComponent<ScaleFit>();
                if (scaleFit != null)
                {
                    scaleFit.FitSize();
                }

                // var scaleFitFrame = 
                // obj.GetOrAddComponent<CanvasFitter>();
                // scaleFitFrame.fitType = CanvasFitter.FitType.ScaleToOuter;
                // if (scaleFitFrame != null)
                // {
                //     scaleFitFrame.FitSize();
                // }
            }


            this._frameChildObj = obj;
        }

        private async Task InitMaskPictureTexture()
        {
            // var sprite = await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.MaskRes);

            var obj = new GameObject
            {
                name = $"{this.TaskData.MaskRes.Replace(".png", "")}"
            };
            obj.transform.SetParent(this._frameObj.transform);
            var image = obj.GetOrAddComponent<Image>();
            image.color = Color.black;
            // var canvas = obj.AddComponent<CanvasGroup>();
            // canvas.alpha = 1;
            obj.transform.rectTransform().sizeDelta = new Vector2(1080, 1920);
            obj.transform.rectTransform().localPosition = Vector3.zero;
            obj.transform.rectTransform().localScale = Vector3.one;

            // image.sprite = sprite;

            this._maskPictureObj = obj;
            var canvas = obj.GetOrAddComponent<CanvasGroup>();
            canvas.alpha = 0;
        }

        private void InitPos()
        {
            this._frameObj.transform.rectTransform().localPosition = this.TaskData.Trans.startPos;
            this._frameObj.transform.rectTransform().localEulerAngles = this.TaskData.Trans.startRotation;
            this._frameObj.transform.rectTransform().localScale = this.TaskData.Trans.startScale;
            var canvas1 = this._frameObj.GetOrAddComponent<CanvasGroup>();
            canvas1.alpha = this.TaskData.Trans.startAlpha;

            if (this._pictureObj != null)
            {
                this._pictureObj.transform.rectTransform().localPosition = this.TaskData.PictureStartPos;
                this._pictureObj.transform.rectTransform().localScale = Vector3.one;
            }

            if (!this.TaskData.OpenPicture)
            {
                this._rawImg.transform.rectTransform().localPosition = this.TaskData.Trans.startPos;
                this._rawImg.transform.rectTransform().localEulerAngles = this.TaskData.Trans.startRotation;
                this._rawImg.transform.rectTransform().localScale = this.TaskData.Trans.startScale;
                var canvas2 = this._rawImg.GetOrAddComponent<CanvasGroup>();
                canvas2.alpha = this.TaskData.Trans.startAlpha;
            }
        }

        private async Task InitPicture()
        {
            if (!this.TaskData.OpenPicture) return;

            var sprite = await this.BucketPicture.GetOrAquireSpriteAsync(this.TaskData.PictureRes);
            var obj = new GameObject
            {
                name = $"{this.TaskData.PictureRes.Replace(".png", "")}"
            };
            var image = obj.GetOrAddComponent<Image>();
            var canvas = obj.AddComponent<CanvasGroup>();
            // canvas.alpha = 1;
            // obj.transform.rectTransform().localPosition = Vector3.zero;
            obj.transform.rectTransform().localPosition = this.TaskData.PictureStartPos;
            obj.transform.rectTransform().localScale = Vector3.one;

            image.sprite = sprite;
            image.SetNativeSize();

            if (this.TaskData.OpenFit)
            {
                await Task.Delay(1);
                var scaleFit = obj.GetOrAddComponent<CanvasFitter>();
                scaleFit.fitType = CanvasFitter.FitType.ScaleToOuter;
                scaleFit.Reporcess();
            }

            this._pictureObj = obj;
            obj.transform.SetParent(this._frameObj.transform);
        }

        public void SaveCacheData()
        {
            PlotRuntimeMaskCacheManager.SaveMaskCacheObj(this.TaskData, this._rawImg, this._frameObj,
                this._pictureObj, this._maskPictureObj);
        }

        // 交换两张图片的位置
        private void ReplaceLayer()
        {
            if (!this.TaskData.OpenPicture) return;

            var rawLayer = this._frameChildObj.transform.GetSiblingIndex();
            var pictureLayer = this._pictureObj.transform.GetSiblingIndex();

            this._frameChildObj.transform.SetSiblingIndex(pictureLayer);
            this._pictureObj.transform.SetSiblingIndex(rawLayer);
        }

        #endregion
    }
}