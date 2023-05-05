using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Plot.Runtime
{
    public class PlotTimelineActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.TimeLine;
        public int TimelineId;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsTimeLineActionElement actionElement = (PlotComicsTimeLineActionElement) element;
            this.TimelineId = actionElement.timelineId;
        }
    }

    public class PlotTimelineActionTask : PlotActionAbilityTask
    {
        public PlotTimelineActionTaskData TaskData => (PlotTimelineActionTaskData) this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);

            this.InitSomeRoot();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);

            this.Update(frameIdx);
        }

        public override  async Task EndExecute()
        {
            base.EndExecute();
            this.OnDestroy();
            // 还原层级
            // this.ReplaceLayer();
        }


        #region ---清理---

        private void OnDestroy()
        {
            // if (this._timelineObj != null)
            // {
            //     Object.Destroy(this._timelineObj);
            // }
            //
            // PlotRuntimeTimelineCacheManager.RemoveTimeline(this.TaskData.TimelineId);
        }

        #endregion

        #region ---初始化---

        private GameObject _timelineObj;
        private PlotTimelineConfigTaskData _configData;
        private Vector3 _startPos;
        private bool _isPlaying = false;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeTimelineCacheManager.GetTimelineCache(this.TaskData.TimelineId);
            if (cacheInfo == null) return;
            this._timelineObj = cacheInfo.TimelineObj;
            this._configData = cacheInfo.ConfigElement;
        }

        #endregion

        #region ---刷新---

        private void Update(int frameIdx)
        {
            if (this._timelineObj == null) return;

            var director = this._timelineObj.GetComponentInChildren<PlayableDirector>();
            if (director == null) return;
            // if (!this._isPlaying)
            // {
            //     director.Play();
            //     this._isPlaying = true;
            // }

            if (frameIdx >= this.TaskData.EndFrame)
            {
                director.Stop();
                return;
            }

            if (frameIdx <= this.TaskData.StartFrame)
            {
                director.time = 0;
                return;
            }

            // var currentTime = (frameIdx - this.TaskData.StartFrame) * 1.0f / PlotDefineUtil.DEFAULT_FRAME_NUM;
            // director.time = currentTime < 0 ? 0 : currentTime;
            // director.Evaluate();
        }

        #endregion
    }
}