using System;
using System.Collections;
using UnityEngine;

using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Net.Http;
using UnityEngine.Networking;

namespace Loxodon.Framework.Bundles
{
    public class FileAsyncBundleLoaderBuilder : AbstractLoaderBuilder
    {
        public FileAsyncBundleLoaderBuilder(Uri baseUri) : base(baseUri)
        {
        }

        public override BundleLoader Create(BundleManager manager, BundleInfo bundleInfo)
        {
            return new FileAsyncBundleLoader(new Uri(this.BaseUri, bundleInfo.Filename), bundleInfo, manager);
        }
    }

    public class FileAsyncBundleLoader : BundleLoader
    {
        private const float WEIGHT = 0.7f;

        public FileAsyncBundleLoader(Uri uri, BundleInfo bundleInfo, BundleManager manager) : base(uri, bundleInfo, manager)
        {
        }

        protected override IEnumerator DoLoadAssetBundle(IProgressPromise<float, AssetBundle> promise)
        {
            if (this.BundleInfo.IsEncrypted)
            {
                promise.UpdateProgress(0f);
                promise.SetException(new NotSupportedException(string.Format("The data of the AssetBundle named '{0}' is encrypted,use the CryptographBundleLoader to load,please.", this.BundleInfo.Name)));
                yield break;
            }

            string path = this.GetAbsolutePath();
#if UNITY_ANDROID && !UNITY_5_4_OR_NEWER
            if (this.Uri.Scheme.Equals("jar", StringComparison.OrdinalIgnoreCase))
            {
                promise.UpdateProgress(0f);
                promise.SetException(new NotSupportedException(string.Format("Failed to load the AssetBundle '{0}' at the address '{1}'.It is not supported before the Unity3d 5.4.0 version.", this.BundleInfo.Name, path)));
                yield break;
            }
#endif
            float weight = 0;
            long totalSize = this.BundleInfo.FileSize;
            if (this.IsRemoteUri())
            {
                weight = WEIGHT;
                string fullname = BundleUtil.GetStorableDirectory() + this.BundleInfo.Filename;
                using (UnityWebRequest www = new UnityWebRequest(path))
                {
                    www.downloadHandler = new DownloadFileHandler(fullname);
#if UNITY_2018_1_OR_NEWER
                    www.SendWebRequest();
#else
                    www.Send();
#endif
                    while (!www.isDone)
                    {
                        if (www.downloadedBytes >= 0 && totalSize > 0)
                            promise.UpdateProgress(weight * (float)www.downloadedBytes / totalSize);
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        promise.SetException(new Exception(string.Format("Failed to load the AssetBundle '{0}' at the address '{1}'.Error:{2}", this.BundleInfo.Name, path, www.error)));
                        yield break;
                    }
                    path = fullname;
                }
            }

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                promise.UpdateProgress(weight + (1 - weight) * request.progress);
                yield return null;
            }

            var assetBundle = request.assetBundle;
            if (assetBundle == null)
            {
                promise.SetException(new Exception(string.Format("Failed to load the AssetBundle '{0}' at the address '{1}'.", this.BundleInfo.Name, path)));
                yield break;
            }

            promise.UpdateProgress(1f);
            promise.SetResult(assetBundle);
        }
    }
}
