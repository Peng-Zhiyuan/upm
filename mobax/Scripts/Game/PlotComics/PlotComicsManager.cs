using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using BattleEngine.View;

namespace Plot.Runtime
{
    public class PlotComicsManager : StuffObject<PlotComicsManager>
    {
        public string CurPlayComicsRes = "";
        
        
        #region ---预加载地图---

        private SceneInstance _sceneInstance;

        public async Task LoadMapSceneAsync()
        {
            var curScene = SceneManager.GetActiveScene();
            var cacheInfo = PlotRuntimeSceneCacheManager.SceneCacheInfo;
            var sceneName = this.GetSceneName();
            // 如果当前激活场景就是此场景
            if (curScene.name.ToUpper().Equals(sceneName.ToUpper()))
            {
                UnityEngine.Debug.LogWarning(curScene.name + " == " + sceneName);
                return;
            }

            if (cacheInfo == null
                || !cacheInfo.IsActive)
            {
                this._sceneInstance = await SceneUtil.AddressableLoadSceneAsync(sceneName, LoadSceneMode.Additive);
                PlotRuntimeSceneCacheManager.SaveCacheSceneInstance(this._sceneInstance);
                SceneManager.SetActiveScene(this._sceneInstance.Scene);
            }

            var partsRoot = this._sceneInstance.Scene.GetRootGameObjects().ToList().Find(val => val.name == "Parts");
            // clear所有的parts節點内瓤
            TransformUtil.RemoveAllChildren(partsRoot.transform);
            var randomMap = partsRoot.GetComponent<RandomMap>();
            randomMap.enabled = false;
        }

        public string GetSceneName()
        {
            return "Env_AKY_Street_New_All";
        }

        public void UnloadMapScene()
        {
            var sceneName = this.GetSceneName();
            var curScene = SceneManager.GetActiveScene();
            if (curScene.name.Equals(sceneName))
            {
                SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
            }
        }

        #endregion

        private Bucket Bucket => BucketManager.Stuff.Plot;

        public async Task LoadComicsPage()
        {
            await Bucket.GetOrAquireAsync<GameObject>($"{nameof(PlotComicsPage)}.prefab");
            await Bucket.GetOrAquireAsync<GameObject>($"{nameof(ConversationPage)}.prefab");
            await Bucket.GetOrAquireAsync<GameObject>($"{nameof(PlotChatSpinePage)}.prefab");
            await Bucket.GetOrAquireAsync<GameObject>($"{nameof(PlotTimelinePage)}.prefab");
        }

        public async Task LoadComicsBGMAsync(List<IPlotEventData> eventRowList)
        {
            foreach (var eventRow in eventRowList)
            {
                await this.LoadComicsBGMAsync(eventRow);
            }
        }

        /// <summary>
        /// 预加载漫画音乐
        /// </summary>
        public async Task LoadComicsBGMAsync(IPlotEventData eventRow)
        {
            // if (string.IsNullOrEmpty(eventRow.Bgm)) return;
            // var address = AudioManager.TryFixAddressExtension(eventRow.Bgm);
            // var bucket = BucketManager.Stuff.Plot;
            // await bucket.GetOrAquireAsync<AudioClip>(address, true);
        }

        #region ---创建资源---

        public async Task<GameObject> CreatorFx(string fxName)
        {
            if (string.IsNullOrEmpty(fxName))
                return null;
            GameObject fxObj = await CreatorFx(fxName, null, Vector3.zero);
            return fxObj;
        }

        public async Task<GameObject> CreatorFx(string fxName, Transform parent, Vector3 offset)
        {
            if (string.IsNullOrEmpty(fxName))
                return null;
            GameObject fxObj = null;
            var fx = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(fxName);
            if (fx == null)
            {
                return null;
            }

            fxObj = GameObject.Instantiate(fx, parent, true);
            ParticleSystemPlayCtr ctr = fxObj.GetComponent<ParticleSystemPlayCtr>();
            if (ctr == null)
            {
                fxObj.AddComponent<ParticleSystemPlayCtr>();
            }

            Chronos.Timeline timeLine = fxObj.GetComponent<Chronos.Timeline>();
            if (timeLine == null)
            {
                timeLine = fxObj.AddComponent<Chronos.Timeline>();
                timeLine.mode = Chronos.TimelineMode.Global;
                timeLine.globalClockKey = "Effect";
            }

            if (parent == null)
            {
                fxObj.transform.position = offset;
            }
            else
            {
                fxObj.transform.localPosition = offset;
            }

            fxObj.transform.localScale = Vector3.one;
            ParticleSystemPlayCtr psCtr = fxObj.GetComponent<ParticleSystemPlayCtr>();
            if (psCtr != null)
            {
                psCtr.Play();
            }
            else
            {
                psCtr = fxObj.AddComponent<ParticleSystemPlayCtr>();
                psCtr.Play();
            }

            psCtr.effectPrefabName = fxName;
            fxObj.SetActive(true);
            return fxObj;
        }

        #endregion

        #region ---预加载资源---

        private PlotPreloadControl _preloadCtrl; // 预加载漫画资源的管理器

        public async Task LoadTimelineRes(List<int> plotIds)
        {
            foreach (var plotId in plotIds)
            {
                var plotRow = PlotDataManager.Stuff.GetPlotEventData(plotId);
                if (plotRow == null || plotRow.Timeline.Length <= 0) continue;
                var address = $"{plotRow.Timeline}.prefab";
                await this.Bucket.GetOrAquireAsync<GameObject>(address);
            }
        }

        public async Task LoadSpineChatBgAssets(List<int> plotIds)
        {
            var preloadList = new List<string>();
            foreach (var plotId in plotIds)
            {
                var plotRow = PlotDataManager.Stuff.GetPlotEventData(plotId);
                if (plotRow == null || plotRow.ChatType != (int) EPlotChatStyleType.Normal) continue;

                var eventRow = PlotDataManager.Stuff.GetPlotEventData(plotId);
                if (eventRow == null) continue;
                var chatId = eventRow.ChatId;
                var chatList = PlotDataManager.Stuff.GetPlotChatData(chatId);
                if (chatList == null || chatList.Count <=0) continue;
                foreach (var plotData in chatList)
                {
                    if (preloadList.Contains(plotData.ChatBg)) continue;
                    if (string.IsNullOrEmpty(plotData.ChatBg)) continue;
                    await this.Bucket.GetOrAquireSpriteAsync($"{plotData.ChatBg}.png");
                    preloadList.Add(plotData.ChatBg);
                }
            }
        }

        public async Task LoadComicsAssets(List<string> comicsAddressList)
        {
            if (_preloadCtrl == null)
            {
                _preloadCtrl = new PlotPreloadControl();
            }

            await _preloadCtrl.Preload(comicsAddressList);
        }

        public (List<IPlotEventData> eventRowList, List<string> comicsAddressList) GetPreloadComicsAddressList(
            int stageId, EPlotEventType plotEventType)
        {
            var comicsAddressList = new List<string>();
            var eventRowList = new List<IPlotEventData>();
            var rowList = PlotDataManager.Stuff.GetPlotManagerData(stageId);
            if (rowList == null)
            {
                return (eventRowList, comicsAddressList);
            }

            foreach (var row in rowList)
            {
                var plotRow = PlotDataManager.Stuff.GetPlotEventData(row.SoleId);
                if (plotRow == null
                    || plotRow.Type != (int) plotEventType
                    || plotRow.Comics.Count <= 0) continue;
                foreach (var comicsAddress in plotRow.Comics)
                {
                    comicsAddressList.Add(comicsAddress);
                }

                eventRowList.Add(plotRow);
            }

            return (eventRowList, comicsAddressList);
        }

        public (List<IPlotEventData> eventRowList, List<string> comicsAddressList) GetPreloadComicsAddressList(
            List<int> plotIds)
        {
            var comicsAddressList = new List<string>();
            var eventRowList = new List<IPlotEventData>();

            foreach (var row in plotIds)
            {
                var plotRow = PlotDataManager.Stuff.GetPlotEventData(row);
                if (plotRow == null || plotRow.Comics.Count <= 0) continue;
                foreach (var comicsAddress in plotRow.Comics)
                {
                    comicsAddressList.Add(comicsAddress);
                }

                eventRowList.Add(plotRow);
            }

            return (eventRowList, comicsAddressList);
        }

        #endregion
    }
}