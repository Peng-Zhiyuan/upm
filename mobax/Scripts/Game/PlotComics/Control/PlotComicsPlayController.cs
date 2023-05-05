using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotComicsPlayController : MonoBehaviour
    {
        private PlotActionAbilityExecution _actionExecution;
        private PlotConfigAbilityExecution _configExecution;

        private PlotComicsFragCtrl _fragCtrl;

        private async void OnEnable()
        {
            this._actionExecution = new PlotActionAbilityExecution(this.transform);
            this._configExecution = new PlotConfigAbilityExecution(this.transform);
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.Plot;
        private List<PlotComicsPreviewInfo> _previewList = new List<PlotComicsPreviewInfo>();

        public async void InitPreviewList(List<PlotComicsPreviewInfo> previewList)
        {
            this._previewList = previewList;
        }

        #endregion

        private Action _onStopped;

        public void AddStoppedListener(Action onStopped)
        {
            this._onStopped = onStopped;
        }

        #region ---播放&结束

        // 依次播放
        public void DoPlay()
        {
            // 先清理一些缓存的Obj
            // PlotRuntimeBubbleCacheManager.ClearBubbleMap();
            // PlotRuntimeModelCacheManager.ClearModelMap();

            this.CurIndex = 0;
            this.IsPlaying = true;
        }

        public void SetSpeedUp(float speed)
        {
            if (this._fragCtrl == null) return;

            this._fragCtrl.SetSpeedUp(speed);
        }
        
        public void SetPause()
        {
            if (this._fragCtrl == null) return;

            this._fragCtrl.SetPause();
        }

        public void SetReplay()
        {
            if (this._fragCtrl == null) return;

            this._fragCtrl.SetReplay();
        }

        public void SetSpeedNormal()
        {
            if (this._fragCtrl == null) return;

            this._fragCtrl.SetSpeedNormal();
        }

        public async void DoNext()
        {
            await this._fragCtrl.OnEnd();
            // this.CurIndex++;
        }

        private int _curIndex = -1;
        public bool IsPlaying { get; private set; }

        private int CurIndex
        {
            get => this._curIndex;
            set
            {
                var oldIndex = this._curIndex;
                if (this._curIndex.Equals(value)) return;
                this._curIndex = value;
                this.DestroyScenePreview(oldIndex);

                if (this._curIndex > this._previewList.Count - 1)
                {
                    this.Stop();
                    return;
                }

                this.Play();
            }
        }

        private async void DestroyScenePreview(int oldIndex)
        {
            if (oldIndex < 0) return;

            // var preview = this._previewList[oldIndex];
            // var oldComicsData = await this.Bucket.GetOrAquireAsync<PlotComicsConfigObject>(preview.comicsRes);

            // 清除除了scene之外的参数
            // this._configExecution.OnDestroy(oldComicsData);
        }

        // 停止
        public void Stop()
        {
            this.IsPlaying = false;
            this._curIndex = -1;
            this._onStopped?.Invoke();
            PlotComicsManager.Stuff.CurPlayComicsRes = "";
        }

        private async void Play()
        {
            var isLast = this._curIndex == this._previewList.Count - 1;
            var preview = this._previewList[this._curIndex];
            var comicsData = await this.Bucket.GetOrAquireAsync<PlotComicsConfigObject>(preview.comicsRes);

            PlotComicsManager.Stuff.CurPlayComicsRes = preview.comicsRes.Replace(".asset","");
            Debug.Log("开启播放" + PlotComicsManager.Stuff.CurPlayComicsRes + "的剧情");

            WwiseEventManager.SendEvent(TransformTable.Comics, $"{PlotComicsManager.Stuff.CurPlayComicsRes}");
            this._fragCtrl = new PlotComicsFragCtrl(comicsData, this._actionExecution, this._configExecution, isLast);
            // 先初始化
            await this._fragCtrl.BeginExecute();
            this._fragCtrl.DoExecute();
            this._fragCtrl.AddOnCompletedAction(this.DoNextStep);
        }

        public string GetCurComicsRes()
        {
            if (this._curIndex > this._previewList.Count - 1 || this._curIndex < 0) return "";
            var preview = this._previewList[this._curIndex];

            return preview.comicsRes;
        }

        #endregion

        private void Update()
        {
            this._fragCtrl?.OnUpdate();
        }

        private async void DoNextStep()
        {
            var previewData = this._previewList[this._curIndex];
            if (previewData.pageNameType != EPlotComicsInteractivePageName.None)
            {
                var pageName = await UIEngine.Stuff.ForwardOrBackToAsync(previewData.pageName);
                UIEngine.Stuff.HookBack(pageName.name, () => { this.CurIndex++; });
                return;
            }

            this.CurIndex++;
        }

        #region ---清理---

        public void Clean()
        {
            var mapRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MAP_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(mapRoot);

            var roleRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MODEL_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(roleRoot);

            var bubbleRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_BUBBLE_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(bubbleRoot);

            var frameRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_FRAME_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(frameRoot);

            var pictureRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_PICTURE_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(pictureRoot);

            var maskPictureRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MASK_PICTURE_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(maskPictureRoot);

            var rawRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_RAWIMG_ROOT_PATH).gameObject;
            rawRoot.SetActive(false);

            var frontRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_FRONT_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(frontRoot);

            var timelineRoot = this.transform.Find(PlotDefineUtil.PLOT_RUNTIME_TIMELINE_ROOT_PATH).gameObject;
            PlotRuntimeUtil.ClearAllChildren(timelineRoot);

            PlotRuntimeBubbleCacheManager.ClearBubbleMap();
            PlotRuntimeModelCacheManager.ClearModelMap();

            this.CleanBucket();
        }

        private void CleanBucket()
        {
            var bucket = BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_PICTURE_BUCKET);
            bucket.ReleaseAll();
        }

        #endregion
    }
}