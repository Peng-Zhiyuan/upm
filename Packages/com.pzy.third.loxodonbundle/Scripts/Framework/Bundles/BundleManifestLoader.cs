﻿using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;

using Loxodon.Log;
using Loxodon.Framework.Execution;
using Loxodon.Framework.Asynchronous;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Networking;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Text.RegularExpressions;
using Ionic.Zip;
#endif
using System.Collections.Generic;

namespace Loxodon.Framework.Bundles
{
#pragma warning disable 0414, 0219
    public class BundleManifestLoader : IBundleManifestLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BundleManifestLoader));

#if UNITY_ANDROID  && !UNITY_EDITOR
        public string GetCompressedFileName(string url)
        {
            url = Regex.Replace(url, @"^jar:file:///", "");
            return url.Substring(0, url.LastIndexOf("!"));
        }

        public string GetCompressedEntryName(string url)
        {
            return url.Substring(url.LastIndexOf("!") + 2);
        }
#endif

        public static Dictionary<string, string> pathToStringDic = new Dictionary<string, string>();

        public static void Cache(string path, string json)
        {
            Debug.Log("[BundleManifestLoader] cache path: " + path);
            pathToStringDic[path] = json;
        }

        public static string GetCache(string path)
        {
            Debug.Log("[BundleManifestLoader] get cache: " + path);
            if(pathToStringDic.ContainsKey(path))
            {
                var ret = pathToStringDic[path];
                Debug.Log("[BundleManifestLoader] find: " + ret);
                return ret;
            }
            Debug.Log("[BundleManifestLoader] not found cache");
            return null;
        }

        /// <summary>
        /// 安卓上文件不存在会返回 null
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual BundleManifest Load(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Regex.IsMatch(path, @"(jar:file:///)|(\.jar)|(\.apk)|(\.obb)|(\.zip)", RegexOptions.IgnoreCase))
            {
                using (ZipFile zip = new ZipFile(this.GetCompressedFileName(path)))
                {
                    string entryName = this.GetCompressedEntryName(path);
                    if (!zip.ContainsEntry(entryName))
                    {
                        if(log.IsErrorEnabled)
                            Debug.LogErrorFormat("Not found the BundleManifest '{0}'.", path);
                        return null;
                    }

                    ZipEntry entry = zip[entryName];
                    byte[] buffer = new byte[entry.UncompressedSize];
                    using (Stream input = entry.OpenReader())
                    {
                        input.Read(buffer, 0, buffer.Length);
                    }
                    var json2 = Encoding.UTF8.GetString(buffer);
                    Cache(path, json2);
                    return BundleManifest.Parse(json2);
                }
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            Cache(path, json);
            return BundleManifest.Parse(json);
#elif UNITY_WEBGL && !UNITY_EDITOR
            throw new NotSupportedException("Because WebGL is single-threaded, this method is not supported,please use LoadAsync instead.");
#else
            return BundleManifest.Parse(File.ReadAllText(path, Encoding.UTF8));
#endif
        }

        public virtual IAsyncResult<BundleManifest> LoadAsync(string path)
        {
            AsyncResult<BundleManifest> result = new AsyncResult<BundleManifest>();
            Executors.RunOnCoroutine(DoLoadAsync(result, path), result);
            return result;
        }

        protected virtual IEnumerator DoLoadAsync(IPromise<BundleManifest> promise, string path)
        {
            string absoluteUri = "";
            try
            {
                Uri uri = new Uri(path);
                absoluteUri = uri.AbsoluteUri;
                if (uri.Scheme.Equals("jar") && !absoluteUri.StartsWith("jar:file://"))
                    absoluteUri = absoluteUri.Replace("jar:file:", "jar:file://");
            }
            catch (Exception)
            {
                absoluteUri = path;
            }
#if UNITY_2017_1_OR_NEWER
            using (UnityWebRequest www = new UnityWebRequest(absoluteUri))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
#if UNITY_2018_1_OR_NEWER
                www.SendWebRequest();
#else
                www.Send();
#endif
                while (!www.isDone)
                {
                    yield return null;
                }

                if (!string.IsNullOrEmpty(www.error))
                {
                    promise.SetException(new Exception(string.Format("Failed to load the Manifest.dat at the address '{0}'.Error:{1}", absoluteUri, www.error)));
                    yield break;
                }

                try
                {
                    string json = www.downloadHandler.text;
                    BundleManifest manifest = BundleManifest.Parse(json);
                    promise.SetResult(manifest);
                }
                catch (Exception e)
                {
                    promise.SetException(e);
                }
            }
#else
            using (WWW www = new WWW(absoluteUri))
            {
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {
                    promise.SetException(new Exception(string.Format("Failed to load the Manifest.dat at the address '{0}'.Error:{1}", absoluteUri, www.error)));
                    yield break;
                }

                try
                {
                    string json = www.text;
                    BundleManifest manifest = BundleManifest.Parse(json);
                    promise.SetResult(manifest);
                }
                catch (Exception e)
                {
                    promise.SetException(e);
                }
            }
#endif
        }
    }
}
