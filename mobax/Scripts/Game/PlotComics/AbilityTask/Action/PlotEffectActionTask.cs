using System.Threading.Tasks;
using BattleEngine.View;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotEffectActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Camera;

        public string EffectPrefabName = "";
        public Vector3 Scale = Vector3.one;
        public EPlotEffectCoordinateType AttachType;
        public string AttachPoint = "";
        public int AttachTargetId = 0;
        public Vector3 PosOffset = Vector3.zero;
        public Vector3 AngleOffset = Vector3.zero;
        public bool IsAttachLookAt = true;
        public bool IsLoop = false;
        public bool IsLookAtTarget;
        public int LookAtTargetId;

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsEffectActionElement actionElement = (PlotComicsEffectActionElement) element;
            this.EffectPrefabName = AddressablePathConst.SkillEditorPathParse(actionElement.effectRes);
            this.Scale = actionElement.scale;
            this.AttachType = actionElement.attachType;
            this.AttachPoint = actionElement.attachPoint;
            this.AttachTargetId = actionElement.attachTargetId;
            this.PosOffset = actionElement.posOffset;
            this.AngleOffset = actionElement.angleOffset;
            this.IsAttachLookAt = actionElement.isAttachLookAt;
            this.IsLoop = actionElement.isloop;
            this.IsLookAtTarget = actionElement.isLookAtTarget;
            this.LookAtTargetId = actionElement.lookAtTargetId;
        }
    }

    public class PlotEffectActionTask : PlotActionAbilityTask
    {
        public PlotEffectActionTaskData TaskData => (PlotEffectActionTaskData) this.TaskInitData;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            this.InitSomeRoot();
            await this.InitEffect();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            this.Update(frameIdx);
        }

        public override async Task EndExecute()
        {
            base.EndExecute();
            this.OnDestroy();
        }

        #region ---初始化---

        private GameObject _effectRoot;
        private GameObject _fx;

        private void InitSomeRoot()
        {
            this._effectRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_EFFECT_ROOT_PATH).gameObject;
        }

        private async Task InitEffect()
        {
            if (this.TaskData == null || string.IsNullOrEmpty(TaskData.EffectPrefabName)) return;
            var cacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.AttachTargetId);
            if (cacheInfo == null) return;
            var actorTrans = cacheInfo.ModelObj.transform;
            switch (this.TaskData.AttachType)
            {
                case EPlotEffectCoordinateType.Global:
                    this._fx = await PlotComicsManager.Stuff.CreatorFx(this.TaskData.EffectPrefabName,
                        this._effectRoot.transform, Vector3.zero);
                    this._fx.name = this.TaskData.EffectPrefabName;

                    if (TaskData.IsAttachLookAt)
                    {
                        this._fx.transform.position = actorTrans.position +
                                                      Quaternion.Euler(actorTrans.eulerAngles) * TaskData.PosOffset;
                        this._fx.transform.forward = Quaternion.Euler(TaskData.AngleOffset) * actorTrans.forward;
                    }
                    else
                    {
                        this._fx.transform.position = actorTrans.position + TaskData.PosOffset;
                        this._fx.transform.rotation = Quaternion.Euler(TaskData.AngleOffset);
                    }

                    this._fx.transform.localScale = TaskData.Scale;
                    break;
                case EPlotEffectCoordinateType.Target:
                    Transform parentTrans = actorTrans;
                    if (string.IsNullOrEmpty(TaskData.AttachPoint))
                    {
                        parentTrans = actorTrans;
                        this._fx = await PlotComicsManager.Stuff.CreatorFx(TaskData.EffectPrefabName, parentTrans,
                            Vector3.zero);
                    }
                    else
                    {
                        parentTrans = GameObjectHelper.FindChild(actorTrans, TaskData.AttachPoint);
                        this._fx = await PlotComicsManager.Stuff.CreatorFx(TaskData.EffectPrefabName, parentTrans,
                            Vector3.zero);
                    }

                    this._fx.name = TaskData.EffectPrefabName;
                    if (TaskData.IsAttachLookAt)
                    {
                        this._fx.transform.localPosition = TaskData.PosOffset;
                        this._fx.transform.localRotation = Quaternion.Euler(TaskData.AngleOffset);
                    }
                    else
                    {
                        this._fx.transform.position = parentTrans.position + TaskData.PosOffset;
                        this._fx.transform.rotation = Quaternion.Euler(TaskData.AngleOffset);
                    }

                    this._fx.transform.localScale = TaskData.Scale;
                    break;
            }
        }

        #endregion

        #region ---更新---

        private void Update(int frameIdx)
        {
            if (this._fx == null) return;

            if (TaskData.IsLookAtTarget)
            {
                var cacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.LookAtTargetId);
                if (cacheInfo == null) return;
                var target = cacheInfo.ModelObj;
                if (this._fx != null && target != null)
                {
                    this._fx.transform.LookAt(target.transform.position);
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (this._fx !=null)
            {
                Object.Destroy(this._fx);
                this._fx = null;
            }
        }
    }
}