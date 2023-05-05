using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plot.Runtime
{
    public class PlotTimelineConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.Timeline;
        public string TimelinePath;
        public int TimelineId;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);
            PlotComicsTimeLineConfigElement configElement = (PlotComicsTimeLineConfigElement) element;
            this.TimelinePath = configElement.timeLinePath;
            this.TimelineId = configElement.timelineId;
        }
    }

    public class PlotTimelineConfigTask : PlotConfigAbilityTask
    {
        public PlotTimelineConfigTaskData TaskData => (PlotTimelineConfigTaskData) this.TaskInitData;

        public override async Task BeginExecute()
        {
            await base.BeginExecute();
            this.InitSomeRoot();
            this.InitTimeline();
            this.ShowPartsActive(false);
        }

        public override async Task EndExecute()
        {
            // this.OnDestroy();
            this.ShowPartsActive(true);
        }

        public override async Task Preload()
        {
            await base.Preload();
            var modelSplit = this.TaskData.TimelinePath.Split('/');
            var timelineRes = modelSplit.Last();
            await this.Bucket.GetOrAquireAsync<GameObject>(timelineRes);
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_3D_ENV_BUCKET);
        private GameObject _timelineRoot;
        private GameObject _timelineObj;

        private void InitSomeRoot()
        {
            this._timelineRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_TIMELINE_ROOT_PATH)
                .gameObject;
        }

        // 如果存在场景 则屏蔽场景中的资源
        private void ShowPartsActive(bool isActive)
        {
            var scene = SceneManager.GetActiveScene();
            var sceneName = PlotComicsManager.Stuff.GetSceneName();
            if (!scene.name.Equals(sceneName)) return;

            var partsRoot = scene.GetRootGameObjects().ToList().Find(val => val.name == "Parts");
            var tempPartsRoot = scene.GetRootGameObjects().ToList().Find(val => val.name == "TempParts");
            var randomMap = partsRoot.GetComponent<RandomMap>();
            randomMap.enabled = false;

            for (int i = 0; i < partsRoot.transform.childCount; i++)
            {
                var go = partsRoot.transform.GetChild(i);
                go.SetActive(isActive);
            }

            for (int i = 0; i < tempPartsRoot.transform.childCount; i++)
            {
                var go = tempPartsRoot.transform.GetChild(i);
                go.SetActive(isActive);
            }
        }

        private async void InitTimeline()
        {
            var timelineSplit = this.TaskData.TimelinePath.Split('/');
            var timelineRes = timelineSplit.Last();
            var obj = await this.Bucket.GetOrAquireAsync<GameObject>(timelineRes);
            var audioListener = obj.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            this._timelineObj = Object.Instantiate(obj, this._timelineRoot.transform);
            this._timelineObj.SetActive(true);
            this._timelineObj.name = $"{timelineRes.Replace(".prefab", "")}_ID_{this.TaskData.TimelineId}";

            PlotRuntimeTimelineCacheManager.AddTimelineCache2Map(this.TaskData.TimelineId, this.TaskData,
                this._timelineObj);
        }

        #endregion
    }
}