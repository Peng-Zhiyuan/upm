using System.Threading.Tasks;
using Coffee.UIEffects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Plot.Runtime
{
    public class PlotMaskDissolveActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.MaskDissolve;
        public string MaskRes;
        public UITransitionEffect.EffectMode DissolveMode = UITransitionEffect.EffectMode.Dissolve;
        public float DissolveWidth = 0f;
        public float DissolveSoftness = 0f;
        public UITransitionEffect.EffectDir EffectDir = UITransitionEffect.EffectDir.Right;
        public string TextureRes;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsMaskDissolveActionElement actionElement = (PlotComicsMaskDissolveActionElement) element;
            this.MaskRes = actionElement.maskRes;
            this.DissolveMode = actionElement.dissolveMode;
            this.DissolveWidth = actionElement.dissolveWidth;
            this.DissolveSoftness = actionElement.dissolveSoftness;
            this.EffectDir = actionElement.effectDir;
            this.TextureRes = actionElement.textureRes;
        }
    }

    public class PlotMaskDissolveActionTask : PlotActionAbilityTask
    {
        public PlotMaskDissolveActionTaskData TaskData => (PlotMaskDissolveActionTaskData) this.TaskInitData;
        private Bucket Bucket => BucketManager.Stuff.Plot;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            this.InitSomeRoot();
            this.InitTotalTime();
            this.InitDissolveSprite();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
        }

        public override  async Task EndExecute()
        {
            base.EndExecute();
        }

        #region ---初始化---

        private float _totalTime = 0;
        private GameObject _dissolve;

        private void InitTotalTime()
        {
            int startFrame = this.TaskData.StartFrame;
            int endFrame = this.TaskData.EndFrame;
            int frameOffset = endFrame - startFrame;
            this._totalTime = frameOffset * 1f / PlotDefineUtil.DEFAULT_FRAME_NUM;
        }

        private void InitSomeRoot()
        {
            this._dissolve = this.ParentRoot.transform.Find("DissolveImage").gameObject;
        }

        private async void InitDissolveSprite()
        {
            var sprite = await this.Bucket.GetOrAquireSpriteAsync(this.TaskData.MaskRes);

            var image = this._dissolve.GetComponent<Image>();
            image.sprite = sprite;

            var effect = this._dissolve.GetComponent<UITransitionEffect>();
            if (effect != null)
            {
                effect.effectDir = this.TaskData.EffectDir;
                effect.effectFactor = 1;
                effect.dissolveWidth = this.TaskData.DissolveWidth;
                effect.dissolveSoftness = this.TaskData.DissolveSoftness;
            }

            var texture = await this.Bucket.GetOrAquireAsync<Texture2D>(this.TaskData.TextureRes);
            effect.transitionTexture = texture;

            DOTween.To(() => effect.effectFactor, x => effect.effectFactor = x, 0, this._totalTime);
        }

        #endregion
    }
}