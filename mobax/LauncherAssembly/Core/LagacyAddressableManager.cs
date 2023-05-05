//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;
//public class LagacyAddressableManager : Single<LagacyAddressableManager>, IUpdatable {
//    private AsyncOperationHandle downloadHandle;
//    private long curDownloadSize = 0;
//    private long loadedSize = 0;
//    private float percent = 0;
//    private float totalPercent = 0;

//    private long totalDownloadSize = 0;
//    public async Task initAsync (bool forceUpdate = false) {
//        await this.initializeAsync();
//        List<string> updateCatalogs = await this.checkForCatalogUpdatesAsync ();
//        // List<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> locators = null;
//        var needUpdate = forceUpdate;
//        if (updateCatalogs != null && updateCatalogs.Count > 0) {
//            Debug.Log ("updateCatalogs.Count:" + updateCatalogs.Count);
//            foreach (var catalog in updateCatalogs) {
//                Debug.Log ("update catalog:" + catalog);
//            }
//            await this.updateCatalogsAsync (updateCatalogs);
//            needUpdate = true;
//        }

//        if (needUpdate) {
//            // long totalDownloadSize = 0;
//            var locators = Addressables.ResourceLocators;
//            foreach (var locator in locators) {
//                foreach (var key in locator.Keys) {
//                    long downloadSize = await this.getDownloadSizeAsync (key);
//                    totalDownloadSize += downloadSize;
//                }
//            }
//            Debug.Log ("totalDownloadSize:" + totalDownloadSize);
//            if(totalDownloadSize == 0) return;
//            if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
//            {
//                foreach (var locator in locators) {
//                    foreach (var key in locator.Keys) {
//                        curDownloadSize = await this.getDownloadSizeAsync (key);
//                        this.downloadHandle = Addressables.DownloadDependenciesAsync (key, true);
//                        //Debug.Log ($"开始下载: {key}");

//                        // long totalDownloadSize = await this.getDownloadSizeAsync (key);
//                        // if (totalDownloadSize > 0) {
//                        //     var isDown = await this.downloadDependencies (key);
//                        //     if (isDown) {
//                        //         Debug.Log ($"download: {key}");
//                        //     }
//                        // }
//                    }
//                }
//                UpdateManager.Stuff.Add (this);
//            }


          
//        }
//    }
//    public void OnUpdate () {
//        if (!downloadHandle.IsDone) {
//            percent = curDownloadSize * downloadHandle.PercentComplete / curDownloadSize;
//            totalPercent = (curDownloadSize + loadedSize)/totalDownloadSize;//curDownloadSize * downloadHandle.PercentComplete / curDownloadSize;
//            Debug.Log("=>percent:"+percent+"   totalPercent:"+totalPercent);
//        }
//        else
//        {
//            totalPercent = (curDownloadSize + loadedSize)/totalDownloadSize;
//            if(totalPercent >= 1)
//            {
//                   Debug.Log("end=>percent:"+percent+"   totalPercent:"+totalPercent);
//                   UpdateManager.Stuff.Remove (this);
//            }
//        }

//    }

//    private Task<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> initializeAsync () {
//        var tcs = new TaskCompletionSource<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> ();
//        Addressables.InitializeAsync ().Completed += (AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> op) => {
//            if (op.Result == null) {
//                Debug.LogError ("Addressables.InitializeAsync failed!");
//                tcs.SetResult (null);
//            }
//            Debug.Log ("Addressables.InitializeAsync succeed!");
//            tcs.SetResult (op.Result);
//        };
//        return tcs.Task;
//    }

//    private Task<List<string>> checkForCatalogUpdatesAsync () {
//        var tcs = new TaskCompletionSource<List<string>> ();
//        Addressables.CheckForCatalogUpdates (true).Completed += (AsyncOperationHandle<List<string>> op) => {
//            if (op.Result == null) {
//                Debug.LogError ("CheckForCatalogUpdates failed!");
//                tcs.SetResult (null);
//            }
//            Debug.Log ("CheckForCatalogUpdates succeed!");
//            tcs.SetResult (op.Result);
//        };
//        return tcs.Task;
//    }

//    private Task<List<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>> updateCatalogsAsync (List<string> needUpdateCatalogs) {
//        var tcs = new TaskCompletionSource<List<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>> ();
//        Addressables.UpdateCatalogs (needUpdateCatalogs, true).Completed += (AsyncOperationHandle<List<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator>> op) => {
//            if (op.Result == null) {
//                Debug.LogError ("UpdateCatalogs failed!");
//                tcs.SetResult (null);
//                return;
//            }
//            Debug.Log ("UpdateCatalogs succeed!");
//            tcs.SetResult (op.Result);
//        };
//        return tcs.Task;
//    }

//    private Task<long> getDownloadSizeAsync (object key) {
//        var tcs = new TaskCompletionSource<long> ();
//        Addressables.GetDownloadSizeAsync (key).Completed += (AsyncOperationHandle<long> op) => {
//            if (op.Result == null) {
//                Debug.LogError ("GetDownloadSizeAsync failed!");
//                tcs.SetResult (0);
//                return;
//            }
//            Debug.Log ("GetDownloadSizeAsync succeed!");
//            tcs.SetResult (op.Result);
//        };
//        return tcs.Task;
//    }

//    private Task<bool> downloadDependencies (object key) {
//        var tcs = new TaskCompletionSource<bool> ();
//        Addressables.DownloadDependenciesAsync (key, true).Completed += (AsyncOperationHandle op) => {
//            if (op.Result == null) {
//                tcs.SetResult (false);
//                return;
//            }
//            tcs.SetResult (true);
//        };
//        return tcs.Task;
//    }

//}