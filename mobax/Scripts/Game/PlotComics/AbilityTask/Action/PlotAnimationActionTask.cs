using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotAnimationActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Animation;
        public string AnimClipName;
        public int Id;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsAnimActionElement actionElement = (PlotComicsAnimActionElement) element;
            this.AnimClipName = actionElement.animClipName;
            this.Id = actionElement.chooseId;
        }
    }

    public class PlotAnimationActionTask : PlotActionAbilityTask
    {
        public PlotAnimationActionTaskData TaskData => (PlotAnimationActionTaskData) this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);

            this.InitSomeRoot();
            this.Play();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
        }

        public override async Task EndExecute()
        {
            this.Stop();
            base.EndExecute();
        }

        #region ---初始化---

        private GameObject _roleObj;
        private Animator _animator;

        private void InitSomeRoot()
        {
            var cacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.Id);
            if (cacheInfo == null) return;
            this._roleObj = cacheInfo.ModelObj;

            if (this._roleObj != null)
            {
                this._animator = this._roleObj.GetComponent<Animator>();
            }
        }

        #endregion

        #region ---动画处理---

        public void Play()
        {
            this._animator.enabled = true;
            this._animator.Play(this.TaskData.AnimClipName);
        }

        public void Stop()
        {
            // if (this._animator != null)
            // {
            //     this._animator.enabled = false;
            // }
        }

        #endregion
    }
}