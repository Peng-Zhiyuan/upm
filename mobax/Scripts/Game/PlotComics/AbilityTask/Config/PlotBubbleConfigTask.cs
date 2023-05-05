using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Plot.Runtime
{
    public class PlotBubbleConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.Bubble;
        public string BubbleRes;
        public int Id;
        public PlotComicsTransBaseSetting Trans;
        public int WordNum = 1;
        public string Word1;
        public string Word2;
        public string WordConfigId1;
        public string WordConfigId2;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);
            PlotComicsBubbleElement configElement = (PlotComicsBubbleElement) element;
            this.BubbleRes = configElement.bubbleRes;
            this.Id = configElement.id;
            this.Trans = configElement.trans;
            this.WordNum = configElement.wordNum;
            this.Word1 = configElement.word1;
            this.Word2 = configElement.word2;
            this.WordConfigId1 = configElement.wordConfigId1;
            this.WordConfigId2 = configElement.wordConfigId2;
        }
    }

    public class PlotBubbleConfigTask : PlotConfigAbilityTask
    {
        public PlotBubbleConfigTaskData TaskData => (PlotBubbleConfigTaskData) this.TaskInitData;

        public override async Task BeginExecute()
        {
            await base.BeginExecute();

            this.InitSomeRoot();
            this.InitBubblePreview();
        }

        public override async Task EndExecute()
        {
            // this.OnDestroy();
            base.EndExecute();
        }

        public override async Task Preload()
        {
            await base.Preload();
            var split = this.TaskData.BubbleRes.Split(new char[] {'_'});
            var id = split.Last().Replace(".png", "");
            await this.Bucket.GetOrAquireAsync<GameObject>($"Bubble_Style_{id}.prefab");
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_MASK_FRAME_BUCKET);
        private GameObject _bubbleRoot;
        private GameObject _bubbleObj;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeMaskCacheManager.MaskCacheInfo;
            this._bubbleRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_BUBBLE_ROOT_PATH).gameObject;
            // this._bubbleRoot = cacheInfo.MaskFrameCacheObj;
        }

        private async void InitBubblePreview()
        {
            var split = this.TaskData.BubbleRes.Split(new char[] {'_'});
            var id = split.Last().Replace(".png", "");
            var obj = await this.Bucket.GetOrAquireAsync<GameObject>($"Bubble_Style_{id}.prefab");
            if (obj == null || this._bubbleRoot == null) return;
            this._bubbleObj = Object.Instantiate(obj, this._bubbleRoot.transform);
            var canvas = this._bubbleObj.GetOrAddComponent<CanvasGroup>();
            this._bubbleObj.transform.rectTransform().localPosition = this.TaskData.Trans.startPos;
            this._bubbleObj.transform.rectTransform().localScale = this.TaskData.Trans.startScale;
            this._bubbleObj.transform.rectTransform().localEulerAngles = this.TaskData.Trans.startRotation;
            canvas.alpha = this.TaskData.Trans.startAlpha;

            PlotRuntimeBubbleCacheManager.AddBubble2Map(this.TaskData.Id, this.TaskData, this._bubbleObj);

            var word1 = this.TaskData.WordConfigId1 == null
                ? default
                : LocalizationManager.Stuff.GetText(this.TaskData.WordConfigId1);
            var word2 = this.TaskData.WordConfigId2 == null
                ? default
                : LocalizationManager.Stuff.GetText(this.TaskData.WordConfigId2);

            var words = new List<string>()
            {
                string.IsNullOrEmpty(word1) ? this.TaskData.Word1 : word1,
                string.IsNullOrEmpty(word2) ? this.TaskData.Word2 : word2,
            };
            this.UpdateBubbleWordList(this._bubbleObj.gameObject, words);
        }

        private void UpdateBubbleWordList(GameObject bubbleObj, List<string> wordList)
        {
            if (wordList == null || wordList.Count <= 0) return;
            var len = wordList.Count;

            var behaviour = bubbleObj.GetComponent<PlotComicsBubbleBehaviour>();
            behaviour.UpdateBubbleWordList(wordList);
        }

        private string ParseChatWord(string word)
        {
            return word.Replace("{username}", Database.Stuff.roleDatabase.Me.name);
        }

        #endregion
    }
}