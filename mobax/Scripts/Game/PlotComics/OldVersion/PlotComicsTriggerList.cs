// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine.ResourceManagement.ResourceProviders;
// using UnityEngine.SceneManagement;
//
// namespace Plot.Runtime
// {
//     public class PlotTriggerList
//     {
//         private List<string> _comicsAddressList;
//         private IPlotEventData _eventRow;
//         private PlotPreloadControl _preloadCtrl;
//
//         private TaskCompletionSource<bool> _tcs;
//
//         public Task WaitCompleteAsync()
//         {
//             this._tcs = new TaskCompletionSource<bool>();
//             return _tcs?.Task;
//         }
//
//         public void SetComicsAddressList(IPlotEventData eventRow)
//         {
//             this._comicsAddressList = eventRow.Comics;
//             this._eventRow = eventRow;
//             this._preloadCtrl = new PlotPreloadControl();
//         }
//
//         /// <summary>
//         /// <=2 则先加载1屏资源
//         /// >2  则先加载2屏资源
//         /// </summary>
//         public async Task PreloadAssets()
//         {
//             // await this.ShowLoadingFloatingAsync();
//             // await this.LoadMapScene();
//             await PlotComicsManager.Stuff.LoadComicsBGMAsync(this._eventRow);
//             // if (this._comicsAddressList.Count <= 2)
//             // {
//             await this._preloadCtrl.Preload(this._comicsAddressList.First());
//             // }
//             // else
//             // {
//             //     var tempAddressList = new List<string>()
//             //     {
//             //         this._comicsAddressList[0],
//             //         this._comicsAddressList[1],
//             //     };
//             //     await this._preloadCtrl.Preload(tempAddressList);
//             // }
//         }
//
//         private async Task<LoadingFloating> ShowLoadingFloatingAsync()
//         {
//             await UIEngine.Stuff.PreLoadFloatingAsync("LoadingFloating");
//             var floating = await UIEngine.Stuff.ShowFloatingAsync("LoadingFloating", null, UILayer.TransitionLayer) as LoadingFloating;
//             return floating;
//         }
//
//         private async void FinishLoading()
//         {
//             var floating = UIEngine.Stuff.FindFloating<LoadingFloating>();
//             floating?.Remove();
//         }
//
//         private SceneInstance _sceneInstance;
//
//         // private async Task LoadMapScene()
//         // {
//         //     var curScene = SceneManager.GetActiveScene();
//         //     var cacheInfo = PlotRuntimeSceneCacheManager.SceneCacheInfo;
//         //     var sceneName = this.GetSceneName();
//         //     // 如果当前激活场景就是此场景
//         //     if (curScene.name.ToUpper().Equals(sceneName.ToUpper()))
//         //     {
//         //         //UnityEngine.Debug.LogError(curScene.name + " == " + sceneName);
//         //         return;
//         //     }
//         //
//         //     if (cacheInfo == null || !cacheInfo.IsActive)
//         //     {
//         //         this._sceneInstance = await SceneUtil.AddressableLoadSceneAsync(sceneName, LoadSceneMode.Additive);
//         //         PlotRuntimeSceneCacheManager.SaveCacheSceneInstance(this._sceneInstance);
//         //         SceneManager.SetActiveScene(this._sceneInstance.Scene);
//         //     }
//         //
//         //     var partsRoot =
//         //         this._sceneInstance.Scene.GetRootGameObjects().ToList().Find(val => val.name == "Parts");
//         //     // clear所有的parts節點内瓤
//         //     TransformUtil.RemoveAllChildren(partsRoot.transform);
//         //     var randomMap = partsRoot.GetComponent<RandomMap>();
//         //     randomMap.enabled = false;
//         // }
//         //
//         // private string GetSceneName()
//         // {
//         //     return "Env_AKY_Street_New_All";
//         // }
//
//         #region ---播放---
//
//         private bool _showSkip = false;
//
//         public async void StartPlay(bool needPreload = true, bool showSkip = false)
//         {
//             this._showSkip = showSkip;
//             if (needPreload)
//             {
//                 await this.PreloadAssets();
//             }
//
//             this._curIndex = -1;
//             this.CurIndex = 0;
//             // if (this._comicsAddressList.Count <= 2)
//             // {
//             this.PreloadNextAssets();
//             // }
//         }
//
//         private int _curIndex = -1;
//
//         private int CurIndex
//         {
//             get => this._curIndex;
//             set
//             {
//                 if (this._curIndex.Equals(value)) return;
//                 this._curIndex = value;
//
//                 if (this._curIndex > this._comicsAddressList.Count - 1)
//                 {
//                     this.Stop();
//                     return;
//                 }
//
//                 this.Play();
//             }
//         }
//
//
//         private PlotComicsPreviewConfigObject _curPreviewData;
//         private static Bucket Bucket => BucketManager.Stuff.Plot;
//
//         private async void Play()
//         {
//             var comicsAddress = this._comicsAddressList[this._curIndex];
//             this._curPreviewData =
//                 await Bucket.GetOrAquireAsync<PlotComicsPreviewConfigObject>($"{comicsAddress}.asset");
//             PlotComicsTrigger.Trigger(this._eventRow.Id, this._curPreviewData, this.DoNextStep, this.FinishLoading,
//                 this._showSkip);
//         }
//
//         #endregion
//
//         private async void Stop()
//         {
//             await SceneUtil.AddressableUnloadSceneAsync(this._sceneInstance);
//             PlotRuntimeSceneCacheManager.RemoveCacheSceneInstance();
//
//             // this.CleanBucket();
//             this._curIndex = -1;
//             this._tcs.SetResult(true);
//         }
//
//         public void CleanBucket()
//         {
//             if (!GuideManager.Stuff.IsGuideProcessing())
//             {
//                 var bucket3D = BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_3D_ENV_BUCKET);
//                 bucket3D.ReleaseAll();
//
//                 var bucketFrame = BucketManager.Stuff.GetBucket(PlotDefineUtil.PLOT_COMICS_MASK_FRAME_BUCKET);
//                 bucketFrame.ReleaseAll();
//
//                 BucketManager.Stuff.Plot.ReleaseAll();
//             }
//         }
//
//         public async void DoNextStep()
//         {
//             // 播放下一屏的剧情
//             this.CurIndex++;
//
//             // 只有2段的话那不需要加载了
//             if (this._comicsAddressList.Count <= 2) return;
//             this.PreloadNextAssets();
//         }
//
//         // TODO: 后续添加一下如果没加载成功就转菊花
//         private async void PreloadNextAssets()
//         {
//             var index = this._curIndex + 1;
//             if (index > this._comicsAddressList.Count - 1) return;
//
//             // 同步加载下一屏的资源  
//             var comicsAddress = this._comicsAddressList[index];
//             await this._preloadCtrl.Preload(comicsAddress);
//         }
//     }
// }