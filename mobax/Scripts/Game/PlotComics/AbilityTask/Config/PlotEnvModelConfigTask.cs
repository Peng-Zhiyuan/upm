using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotEnvModelConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.SceneModel;
        public string ModelRes;
        public Vector3 Pos;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
        public string ActionName;
        public int Id;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);
            PlotComicsSceneEnvModelElement configElement = (PlotComicsSceneEnvModelElement)element;
            this.ModelRes = configElement.modelRes;
            this.Pos = configElement.pos;
            this.Rotation = configElement.rotation;
            this.Scale = configElement.scale;
            this.ActionName = configElement.actionName;
            this.Id = configElement.id;
        }
    }

    public class PlotEnvModelConfigTask : PlotConfigAbilityTask
    {
        public PlotEnvModelConfigTaskData TaskData => (PlotEnvModelConfigTaskData)this.TaskInitData;

        public override async Task BeginExecute()
        {
            await base.BeginExecute();

            this.InitSomeRoot();
            await this.InitModel();
        }

        public override  async Task EndExecute()
        {
            this.OnDestroy();
            base.EndExecute();
        }

        public override async Task Preload()
        {
            await base.Preload();
            var modelSplit = this.TaskData.ModelRes.Split('/');
            var modelRes = modelSplit.Last();
            await this.Bucket.GetOrAquireAsync<GameObject>(modelRes);
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_3D_ENV_BUCKET);
        private GameObject _roleRoot;
        private GameObject _roleObj;

        private void InitSomeRoot()
        {
            this._roleRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MODEL_ROOT_PATH).gameObject;
        }

        /// <summary>
        /// 初始化模型
        /// </summary>
        // 初始化背景
        private async Task InitModel()
        {
            var modelSplit = this.TaskData.ModelRes.Split('/');
            var modelRes = modelSplit.Last();
            var obj = await this.Bucket.GetOrAquireAsync<GameObject>(modelRes);
            if (obj == null) return;
            this._roleObj = Object.Instantiate(obj, this._roleRoot.transform);
            this._roleObj.SetActive(true);
            this._roleObj.name = $"{modelRes.Replace(".prefab", "")}_ID_{this.TaskData.Id}";

            PlotRuntimeModelCacheManager.AddModel2Map(this.TaskData.Id, this.TaskData, this._roleObj);

            var trans = this._roleObj.transform;
            trans.localPosition = this.TaskData.Pos + PlotDefineUtil.ADD_SCENE_OFFSET;
            trans.localRotation = Quaternion.Euler(this.TaskData.Rotation.x, this.TaskData.Rotation.y,
                this.TaskData.Rotation.z);
            trans.localScale = this.TaskData.Scale;

            this.Stand();
        }

        private void Stand()
        {
            var animator = this._roleObj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("stand");
            }
        }

        #endregion

        #region ---清理---

        private void OnDestroy()
        {
            if (this._roleObj != null)
            {
                Object.Destroy(this._roleObj);
            }

            PlotRuntimeModelCacheManager.RemoveModel(this.TaskData.Id);
        }

        #endregion
    }
}