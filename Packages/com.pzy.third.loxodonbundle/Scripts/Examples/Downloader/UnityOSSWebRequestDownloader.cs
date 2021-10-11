using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Bundles;
using Loxodon.Framework.Net.Http;
using Loxodon.Log;
using UnityEngine;
using UnityEngine.Networking;

namespace Loxodon.Framework.Examples.Bundle
{
    public class UnityOSSWebRequestDownloader : AbstractDownloader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UnityOSSWebRequestDownloader));


        public UnityOSSWebRequestDownloader(Uri baseUri) : base(baseUri, SystemInfo.processorCount * 2) {

        }

        protected override IEnumerator DoDownloadBundles(IProgressPromise<Progress, bool> promise, List<BundleInfo> bundles)
        {
            long totalSize = 0;
            long downloadedSize = 0;
            Progress progress = new Progress();
            List<BundleInfo> list = new List<BundleInfo>();
            for (int i = 0; i < bundles.Count; i++)
            {
                var info = bundles[i];
                totalSize += info.FileSize;
                if (BundleUtil.Exists(info))
                {
                    downloadedSize += info.FileSize;
                    continue;
                }
                list.Add(info);
            }

            progress.TotalCount = bundles.Count;
            progress.CompletedCount = bundles.Count - list.Count;
            progress.TotalSize = totalSize;
            progress.CompletedSize = downloadedSize;
            yield return null;

            List<KeyValuePair<BundleInfo, UnityWebRequest>> tasks = new List<KeyValuePair<BundleInfo, UnityWebRequest>>();
            for (int i = 0; i < list.Count; i++)
            {
                BundleInfo bundleInfo = list[i];
                string fullname = BundleUtil.GetStorableDirectory() + bundleInfo.Filename;
                UnityWebRequest www = new UnityWebRequest(this.GetUrl(bundleInfo.Filename));
                www.downloadHandler = new DownloadFileHandler(fullname);

#if UNITY_2018_1_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif
                tasks.Add(new KeyValuePair<BundleInfo, UnityWebRequest>(bundleInfo, www));

                while (tasks.Count >= this.MaxTaskCount || (i == list.Count - 1 && tasks.Count > 0))
                {
                    long tmpSize = 0;
                    for (int j = tasks.Count - 1; j >= 0; j--)
                    {
                        var task = tasks[j];
                        BundleInfo _bundleInfo = task.Key;
                        UnityWebRequest _www = task.Value;

                        if (!_www.isDone)
                        {
                            tmpSize += (long)Math.Max(0, _www.downloadedBytes);//the UnityWebRequest.downloadedProgress has a bug in android platform
                            continue;
                        }

                        progress.CompletedCount += 1;
                        tasks.RemoveAt(j);
                        downloadedSize += _bundleInfo.FileSize;
#if UNITY_2018_1_OR_NEWER
                        if (_www.isNetworkError)
#else
                        if (_www.isError)
#endif
                        {
                            promise.SetException(new Exception(_www.error));
                            if (log.IsErrorEnabled)
                                Debug.LogErrorFormat("Downloads AssetBundle '{0}' failure from the address '{1}'.Reason:{2}", _bundleInfo.FullName, GetAbsoluteUri(_bundleInfo.Filename), _www.error);
                            _www.Dispose();

                            try
                            {
                                foreach (var kv in tasks)
                                {
                                    kv.Value.Dispose();
                                }
                            }
                            catch (Exception) { }
                            yield break;
                        }
                        _www.Dispose();
                    }

                    progress.CompletedSize = downloadedSize + tmpSize;
                    promise.UpdateProgress(progress);

                    yield return null;
                }
            }
            promise.SetResult(true);
        }

        protected override IEnumerator DoDownloadManifest(string relativePath, IProgressPromise<Progress, BundleManifest> promise) {
            Progress progress = new Progress();
            promise.UpdateProgress(progress);
            byte[] data;
            string path = this.GetUrl(relativePath);

#if UNITY_2017_1_OR_NEWER
            using (UnityWebRequest www = new UnityWebRequest(path))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
#if UNITY_2018_1_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif
                while (!www.isDone)
                {
                    if (www.downloadProgress >= 0)
                    {
                        if (progress.TotalSize <= 0)
                            progress.TotalSize = (long)(www.downloadedBytes / www.downloadProgress);
                        progress.CompletedSize = (long)www.downloadedBytes;
                        promise.UpdateProgress(progress);
                    }
                    yield return null;
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    promise.SetException(new Exception(www.error));
                    yield break;
                }

                data = www.downloadHandler.data;
            }
#else
            using (WWW www = new WWW(path))
            {
                while (!www.isDone)
                {
                    if (www.bytesDownloaded > 0f)
                    {
                        if (progress.TotalSize <= 0)
                            progress.TotalSize = (long)(www.bytesDownloaded / www.progress);
                        progress.CompletedSize = www.bytesDownloaded;
                        promise.UpdateProgress(progress);
                    }
                    yield return null;
                }

                progress.CompletedSize = www.bytesDownloaded;
                promise.UpdateProgress(progress);

                if (!string.IsNullOrEmpty(www.error))
                {
                    promise.SetException(new Exception(www.error));
                    yield break;
                }

                data = www.bytes;
            }
#endif

            try
            {
                BundleManifest manifest = BundleManifest.Parse(Encoding.UTF8.GetString(data));

                FileInfo file = new FileInfo(BundleUtil.GetStorableDirectory() + relativePath);
                if (file.Exists)
                {
                    FileInfo bakFile = new FileInfo(BundleUtil.GetStorableDirectory() + relativePath + ".bak");
                    if (bakFile.Exists)
                        bakFile.Delete();

                    file.CopyTo(bakFile.FullName);
                }

                if (!file.Directory.Exists)
                    file.Directory.Create();

                File.WriteAllBytes(file.FullName, data);
                promise.SetResult(manifest);
            }
            catch (IOException e)
            {
                promise.SetException(e);
            }
        }

        public float GetDownLoadSize(List<BundleInfo> bundles, UNIT unit = UNIT.BYTE) {
            long totalSize = 0;
            for (int i = 0; i < bundles.Count; i++)
            {
                var info = bundles[i];
                if (BundleUtil.Exists(info)) {
                    continue;
                }
                totalSize += info.FileSize;
            }
            
            switch (unit) {
                case UNIT.KB:
                    return totalSize / 1024f;
                case UNIT.MB:
                    return totalSize / 1048576f;
                case UNIT.GB:
                    return totalSize / 1073741824f;
                default:
                    return totalSize;
            }
        }


        string GetUrl(string fileName) 
        {
            return $"{BaseUri}/{fileName}";
        }
        

    }
}
