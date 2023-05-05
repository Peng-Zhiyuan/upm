using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleSystem.Core;
using BattleSystem.ProjectCore;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Plot.Runtime
{
    public class PlotMapConfigTaskData : PlotConfigAbilityTaskData
    {
        public EPlotComicsElementType ElementType = EPlotComicsElementType.SceneMap;
        public string MapRes;
        public Vector3 StartAreaVec;
        public Vector3 EndAreaVec;

        public override void Init(PlotComicsConfigElementItem element)
        {
            base.Init(element);
            PlotComicsSceneMapElement configElement = (PlotComicsSceneMapElement) element;
            this.MapRes = configElement.mapRes;
            this.StartAreaVec = configElement.satrtAreaVec;
            this.EndAreaVec = configElement.endAreaVec;
        }
    }

    public class PlotMapConfigTask : PlotConfigAbilityTask
    {
        public PlotMapConfigTaskData TaskData => (PlotMapConfigTaskData) this.TaskInitData;

        private bool _showFar = true;

        // 是否显示远景
        public void SetShowFarPart(bool showFar)
        {
            this._showFar = showFar;
        }

        public override async Task BeginExecute()
        {
            await base.BeginExecute();

            this.InitSomeRoot();
            // this.InitMapByScene();
            this._showFar = !Battle.Instance.IsFight || (Battle.Instance.IsFight && !Battle.Instance.isEnterCompelted);
            await this.InitMap(false);
        }

        public override async Task EndExecute()
        {
            this.OnDestroy();
            base.EndExecute();
        }

        public override async Task Preload()
        {
            await base.Preload();
            this._sp = this.TaskData.StartAreaVec;
            this._ep = this.TaskData.EndAreaVec;
            await this.InitMap(true);
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_3D_ENV_BUCKET);

        private GameObject _mapRoot;
        private Vector3 _sp;
        private Vector3 _ep;
        private GameObject _parentPartRoot;
        private SceneMapPartsConfig _partsConfig;

        private void InitSomeRoot()
        {
            this._sp = this.TaskData.StartAreaVec;
            this._ep = this.TaskData.EndAreaVec;
            this._mapRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_MAP_ROOT_PATH).gameObject;
        }

        // 根据场景加载
        private async void InitMapByScene()
        {
            var scene = SceneManager.GetActiveScene();
            this._parentPartRoot = scene.GetRootGameObjects().ToList().Find(val => val.name == "Parts");
            var randomMap = this._parentPartRoot.GetComponent<RandomMap>();
            randomMap.enabled = false;
            var configObj = scene.GetRootGameObjects().ToList()
                .Find(val => val.name == "TempParts");
            this._partsConfig = configObj.GetComponent<SceneMapPartsConfig>();
            await this.InitMap(false);
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        private async Task InitMap(bool preload = false)
        {
            var mapResSplit = this.TaskData.MapRes.Split(new char[] {'/'});
            var mapRes = mapResSplit.Last();
            var sceneData = await this.Bucket.GetOrAquireAsync<StageSceneData>(mapRes,true);
            if (sceneData == null) return;
            await this.InitMap(sceneData, preload);
        }

        private async Task InitMap(StageSceneData sceneData, bool preload = false)
        {
            if (this._sp.Equals(this._ep) && !preload) return;

            if (this._showFar && sceneData.sceneInfo.farPart != "")
            {
                await this.InstantiateBgObj(sceneData.sceneInfo.farPart, preload);
            }

            await this.InitMapParts(sceneData.sceneInfo.mapPartConfigs, preload);
            await this.InitEnvironmentParts(sceneData.sceneInfo.environmentPartConfigs, preload);
        }

        private async Task InitMapParts(List<MapPartConfig> mapParts, bool preload)
        {
            var a = new Tuple<Vector3, Vector3>(this._sp, this._ep);
            foreach (var mapPart in mapParts)
            {
                var b = new Tuple<Vector3, Vector3>(mapPart.RectSp, mapPart.RectEp);
                if (!PlotRuntimeUtil.JudgeOverlaps(a, b)) continue;

                await this.InstantiatePart(mapPart, preload);
            }
        }

        private async Task InitEnvironmentParts(List<EnvironmentPartConfig> environmentParts, bool preload)
        {
            var a = new Tuple<Vector3, Vector3>(this._sp, this._ep);

            foreach (var mapPart in environmentParts)
            {
                var b = new Tuple<Vector3, Vector3>(mapPart.RectSp, mapPart.RectEp);
                if (!PlotRuntimeUtil.JudgeOverlaps(a, b)) continue;

                await this.InstantiateEnvironmentPartObj(mapPart, preload);
            }
        }

        // 初始化背景
        private async Task InstantiateBgObj(string bgAddress, bool preload)
        {
            if (preload)
            {
                await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(bgAddress));
                return;
            }

            var obj = await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(bgAddress));
            var gObj = Object.Instantiate(obj, this._mapRoot.transform);
            var cameraRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_CAMERA_PATH).gameObject;
            EnvEffectGroup.Stuff.OpenEffect(cameraRoot.GetComponent<Camera>());
            gObj.SetActive(true);
        }

        private async Task InstantiatePart(MapPartConfig mapPart, bool preload)
        {
            if (preload)
            {
                await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(mapPart.AssetAddress));
                return;
            }

            // var configObj = DictionaryUtil.TryGet(this._partsConfig.configData, mapPart.AssetAddress, default);
            var configObj =
                await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(mapPart.AssetAddress));
            if (configObj == null) return;
            var gObj = Object.Instantiate(configObj, this._mapRoot.transform);

            var gridSize = PlotDefineUtil.GRID_SIZE;
            var position = new Vector3(mapPart.Position.x * gridSize.x, mapPart.Position.y * gridSize.y,
                               mapPart.Position.z * gridSize.z)
                           + PlotDefineUtil.ADD_SCENE_OFFSET;
            gObj.SetActive(true);
            gObj.transform.localPosition = position;
            gObj.transform.localEulerAngles = mapPart.Rotation;
        }

        private async Task InstantiateEnvironmentPartObj(EnvironmentPartConfig info, bool preload)
        {
            if (preload)
            {
                await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(info.AssetAddress));
                return;
            }

            // var configObj = DictionaryUtil.TryGet(this._partsConfig.configData, info.AssetAddress, default);
            var configObj =
                await this.Bucket.GetOrAquireAsync<GameObject>(this.GetMapPartFullAddress(info.AssetAddress));
            if (configObj == null) return;
            var gObj = Object.Instantiate(configObj, this._mapRoot.transform);

            var gridSize = PlotDefineUtil.GRID_SIZE;
            var position = new Vector3(info.Position.x * gridSize.x, info.Position.y * gridSize.y,
                info.Position.z * gridSize.z);
            gObj.SetActive(true);
            gObj.transform.localPosition = position + PlotDefineUtil.ADD_SCENE_OFFSET;
            gObj.transform.localEulerAngles = info.Rotation;
        }

        /// <summary>
        /// 根据简易地址找到正确地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string GetMapPartFullAddress(string address)
        {
            // var split = address.Split(new char[] {'_'});
            // var parentFolder = split[2];
            return $"{address}.prefab";
        }

        #endregion

        #region ---清理---

        private void OnDestroy()
        {
            PlotRuntimeUtil.ClearAllChildren(this._mapRoot);
            PlotRuntimeUtil.ClearAllChildren(this._parentPartRoot, "Logic");
            // SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
        }

        #endregion
    }
}