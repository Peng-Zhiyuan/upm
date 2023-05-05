//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.IO;
//using LuaInterface;
//using UObject = UnityEngine.Object;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using UnityEngine.U2D;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class AssetBundleInfo
//{
//    public AssetBundle m_AssetBundle;
//    public int m_ReferencedCount;

//    public AssetBundleInfo(AssetBundle assetBundle)
//    {
//        m_AssetBundle = assetBundle;
//        m_ReferencedCount = 0;
//    }
//}

//namespace LuaFramework
//{

//    public class ResourceManager : Singleton<ResourceManager>
//    {
//        MonoBehaviour _mono = null;

//        string m_BaseDownloadingURL = "";
//        string[] m_AllManifest = null;
//        AssetBundleManifest m_AssetBundleManifest = null;
//        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
//        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
//        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();
//        List<string> m_loadingDependencies = new List<string>();

//        //同步加载
//        private string[] m_Variants = { };
//        private AssetBundleManifest manifest;
//        private AssetBundle shared, assetbundle;
//        private Dictionary<string, AssetBundle> syncBundles;

//        class LoadAssetRequest
//        {
//            public Type assetType;
//            public string[] assetNames;
//            public LuaTable luaTab;
//            public LuaFunction luaFunc;
//            public Action<UObject[]> sharpFunc;
//        }

//        public void Init(MonoBehaviour mono)
//        {
//            _mono = mono;
//        }


//        // Load AssetBundleManifest.
//        public void Initialize(string manifestName, Action initOK)
//        {
//            //同步加载
//            syncBundles = new Dictionary<string, AssetBundle>();

//            //异步加载
//            m_BaseDownloadingURL = Util.GetRelativePath();
//            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs)
//            {
//                if (objs.Length > 0)
//                {
//                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
//                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
//                }
//                if (initOK != null) initOK();
//            });
//        }

//        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func)
//        {
//            LoadAsset<GameObject>(abName, new string[] { assetName }, func);
//        }

//        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func)
//        {
//            LoadAsset<GameObject>(abName, assetNames, func);
//        }

//        public void LoadPrefab(string abName, string[] assetNames, LuaTable table, LuaFunction func)
//        {
//            LoadAsset<GameObject>(abName, assetNames, null, table, func);
//        }

//        public void LoadScene(string abName, string sceneName, Action<UObject[]> func)
//        {
//            LoadAsset<Scene>(abName, new string[] { sceneName }, func);
//        }
//        public void LoadSprite(string abName, string assetName, Action<UObject[]> func)
//        {
//            LoadAsset<Sprite>(abName, new string[] { assetName }, func);
//        }
//        public void LoadSprites(string abName, string[] assetNames, Action<UObject[]> func)
//        {
//            LoadAsset<Sprite>(abName, assetNames, func);
//        }
//        public void LoadConfig(string abName, string assetName, Action<UObject[]> func)
//        {
//            LoadAsset<TextAsset>(abName, new string[] { assetName }, func);
//        }
//        public void LoadMaterial(string abName, string assetName, Action<UObject[]> func)
//        {
//            LoadAsset<Material>(abName, new string[] { assetName }, func);
//        }
//        public string LoadConfigToString(string assetName)
//        {
//            string info = "";
//#if UNITY_EDITOR
//            StreamReader sr = new StreamReader(Application.dataPath + "/Res/Config/" + assetName + ".json");
//            if (sr == null)
//            {
//                return info;
//            }
//            info = sr.ReadToEnd();
//#else
//                TextAsset s = LoadAssetSync<TextAsset>("config", assetName);
//                info = s.text;              
//#endif
//            return info;
//        }

//        string GetRealAssetPath(string abName)
//        {
//            if (abName.Equals(AppConst.AssetDir))
//            {
//                return abName;
//            }
//            abName = abName.ToLower();
//            if (!abName.EndsWith(AppConst.ExtName))
//            {
//                //目前先特殊处理,之后要改
//                if (!abName.StartsWith("assets"))
//                {
//                    abName += AppConst.ExtName;
//                }

//            }
//            if (abName.Contains("/"))
//            {
//                return abName;
//            }
//            //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
//            for (int i = 0; i < m_AllManifest.Length; i++)
//            {
//                int index = m_AllManifest[i].LastIndexOf('/');
//                string path = m_AllManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
//                if (path.Equals(abName))
//                {
//                    return m_AllManifest[i];
//                }
//            }
//            Debug.LogError("GetRealAssetPath Error:>>" + abName);
//            return null;
//        }

//        string GetDeveloperAssetPath(string abName)
//        {
//            if (abName.Equals(AppConst.AssetDir))
//            {
//                return abName;
//            }
//            abName = abName.ToLower();
//            if (abName.EndsWith(AppConst.ExtName))
//            {
//                abName = abName.Replace(AppConst.ExtName, "");
//            }
//            return abName;
//        }

//        /// <summary>
//        /// 载入素材
//        /// </summary>
//        public void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaTable table = null, LuaFunction func = null)
//        {
//            if (AppConst.EditResLoadMode)
//            {
//                abName = GetRealAssetPath(abName);
//            }

//            //目前先特殊处理,之后要改
//            else if (abName.StartsWith("assets") || abName.StartsWith("assets"))
//            {
//                abName = GetRealAssetPath(abName);
//            }
//            else
//            {
//#if UNITY_EDITOR
//                abName = GetDeveloperAssetPath(abName);
//#else
//                abName = GetRealAssetPath(abName);
//#endif
//            }

//            //Debug.Log("abName__path___" + abName);
//            LoadAssetRequest request = new LoadAssetRequest();
//            request.assetType = typeof(T);
//            request.assetNames = assetNames;
//            request.luaTab = table;
//            request.luaFunc = func;
//            request.sharpFunc = action;

//            List<LoadAssetRequest> requests = null;
//            if (!m_LoadRequests.TryGetValue(abName, out requests))
//            {
//                requests = new List<LoadAssetRequest>();
//                requests.Add(request);
//                m_LoadRequests.Add(abName, requests);

//                if (AppConst.EditResLoadMode)
//                {
//                    _mono.StartCoroutine(OnLoadAsset<T>(abName));
//                }
//                else
//                {
//#if UNITY_EDITOR
//                    if (abName == AppConst.AssetDir)
//                    {
//                        _mono.StartCoroutine(OnLoadAsset<T>(abName));
//                    }
//                    //目前先特殊处理,之后要改
//                    else if (abName.StartsWith("assets") || abName.StartsWith("assets"))
//                    {
//                        _mono.StartCoroutine(OnLoadAsset<T>(abName));
//                    }
//                    else
//                    {
//                        _mono.StartCoroutine(OnLoadDeveloperAsset<T>(abName));
//                    }
//#else
//                    _mono.StartCoroutine(OnLoadAsset<T>(abName));
//#endif
//                }
//            }
//            else
//            {
//                requests.Add(request);
//            }
//        }

//        IEnumerator OnLoadDeveloperAsset<T>(string abName)
//        {
//            List<LoadAssetRequest> list = null;
//            if (!m_LoadRequests.TryGetValue(abName, out list))
//            {
//                m_LoadRequests.Remove(abName);
//                yield break;
//            }

//            string realAbName = abName;
//            if (abName.Contains("effect"))
//            {
//                abName = "effect";
//            }

//            string path = AppConst.ResDir + abName.Replace("_", "/");
//            //Debug.Log("path___" + path);

//            for (int i = 0; i < list.Count; i++)
//            {
//                string[] assetNames = list[i].assetNames;
//                List<UObject> result = new List<UObject>();

//                for (int j = 0; j < assetNames.Length; j++)
//                {
//#if UNITY_EDITOR
//                    string assetPath = assetNames[j];
//                    //string typeStr = TypeTraits<T>.GetTypeName();
//                    //Debug.Log("typeStr___" + typeStr);

//                    if (list[i].assetType == typeof(Sprite))
//                    {
//                        string filePath = "";
//                        string fileName = "";
//                        UObject obj = null;
//                        if (!assetPath.Contains("/"))
//                        {
//                            filePath = path + "/Texture/" + assetPath + ".png";
//                            obj = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
//                        }
//                        else
//                        {
//                            string[] names = assetPath.Split('/');
//                            filePath = path + "/Texture/" + names[0] + ".png";
//                            fileName = names[1];

//                            UObject[] objs = AssetDatabase.LoadAllAssetsAtPath(filePath);
//                            yield return objs;

//                            foreach (var item in objs)
//                            {
//                                if (item.name == fileName)
//                                {
//                                    obj = item;
//                                    break;
//                                }
//                            }
//                        }
//                        yield return obj;
//                        if (obj == null)
//                        {
//                            //todo 暂时保留从Texture读取资源
//                            filePath = path + "/Texture/" + assetPath + ".png";
//                            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
//                            yield return sp;
//                            if (sp != null)
//                            {
//                                result.Add(sp);
//                            }
//                        }
//                        else
//                        {
//                            result.Add(obj);
//                        }

//                        if (obj == null)
//                        {
//                            Debug.Log("文件不存在__path___" + fileName);
//                        }
//                    }
//                    else if (list[i].assetType == typeof(Scene))
//                    {
//                        //不需要做处理
//                    }
//                    else if (list[i].assetType == typeof(TextAsset))
//                    {

//                    }
//                    else
//                    {
//                        string filePath = path + "/Prefab/" + assetPath + ".prefab";
//                        UObject obj = AssetDatabase.LoadAssetAtPath<UObject>(filePath);
//                        yield return obj;

//                        if (obj == null)
//                        {
//                            Debug.LogError("缺少资源___Prefab___" + filePath);
//                        }
//                        else
//                        {
//                            result.Add(obj);
//                        }
//                    }
//#endif
//                }

//                if (list[i].sharpFunc != null)
//                {
//                    list[i].sharpFunc(result.ToArray());
//                    list[i].sharpFunc = null;
//                }
//                if (list[i].luaFunc != null)
//                {
//                    list[i].luaFunc.Call(list[i].luaTab, (object)result.ToArray());
//                    list[i].luaFunc.Dispose();
//                    list[i].luaFunc = null;
//                }
//            }
//            m_LoadRequests.Remove(realAbName);
//        }

//        IEnumerator OnLoadAsset<T>(string abName)
//        {
//            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
//            if (bundleInfo == null)
//            {
//                yield return _mono.StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));

//                bundleInfo = GetLoadedAssetBundle(abName);
//                if (bundleInfo == null)
//                {
//                    m_LoadRequests.Remove(abName);
//                    Debug.LogError("OnLoadAsset--->>>" + abName);
//                    yield break;
//                }
//            }
//            List<LoadAssetRequest> list = null;
//            if (!m_LoadRequests.TryGetValue(abName, out list))
//            {
//                m_LoadRequests.Remove(abName);
//                yield break;
//            }
//            for (int i = 0; i < list.Count; i++)
//            {
//                //如果是整包下载
//                if (list[i].assetNames == null)
//                {
//                    List<UnityEngine.Object> result = new List<UnityEngine.Object>();
//                    AssetBundle ab = bundleInfo.m_AssetBundle;
//                    AssetBundleRequest request = ab.LoadAllAssetsAsync();
//                    yield return request;

//                    foreach (UnityEngine.Object obj in request.allAssets)
//                    {
//                        result.Add(obj);
//                    }

//                    if (list[i].sharpFunc != null)
//                    {
//                        list[i].sharpFunc(result.ToArray());
//                        list[i].sharpFunc = null;
//                    }
//                    bundleInfo.m_ReferencedCount++;
//                }
//                else
//                {
//                    string[] assetNames = list[i].assetNames;
//                    List<UObject> result = new List<UObject>();

//                    AssetBundle ab = bundleInfo.m_AssetBundle;
//                    if (!ab.isStreamedSceneAssetBundle)
//                    {
//                        for (int j = 0; j < assetNames.Length; j++)
//                        {
//                            string assetPath = assetNames[j];
//                            string atlasPath = "";
//                            AssetBundleRequest request;
//                            if (list[i].assetType == typeof(Sprite))
//                            {
//                                //读图集
//                                if (assetPath.Contains("/"))
//                                {
//                                    string[] names = assetPath.Split('/');

//                                    request = ab.LoadAssetWithSubAssetsAsync(names[0], list[i].assetType);
//                                    atlasPath = names[1];
//                                }
//                                else
//                                {
//                                    request = ab.LoadAssetAsync(assetPath, list[i].assetType);
//                                }
//                            }
//                            else
//                            {
//                                request = ab.LoadAssetAsync(assetPath, list[i].assetType);
//                            }
//                            yield return request;

//                            if (list[i].assetType == typeof(Sprite))
//                            {
//                                if (request != null)
//                                {
//                                    //图集
//                                    if (atlasPath != "")
//                                    {
//                                        foreach (var item in request.allAssets)
//                                        {
//                                            if (item.name == atlasPath)
//                                            {
//                                                result.Add(item);
//                                                break;
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        result.Add(request.asset);
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                result.Add(request.asset);
//                            }

//                            //T assetObj = ab.LoadAsset<T> (assetPath);
//                            //result.Add(assetObj);
//                        }
//                    }

//                    if (list[i].sharpFunc != null)
//                    {
//                        list[i].sharpFunc(result.ToArray());
//                        list[i].sharpFunc = null;
//                    }
//                    if (list[i].luaFunc != null)
//                    {
//                        list[i].luaFunc.Call(list[i].luaTab, (object)result.ToArray());
//                        list[i].luaFunc.Dispose();
//                        list[i].luaFunc = null;
//                    }
//                    bundleInfo.m_ReferencedCount++;
//                }

//            }
//            m_LoadRequests.Remove(abName);
//        }

//        IEnumerator OnLoadAssetBundle(string abName, Type type)
//        {
//            string url = m_BaseDownloadingURL + abName;

//            WWW download = null;
//            if (type == typeof(AssetBundleManifest))
//                download = new WWW(url);
//            else
//            {
//                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
//                if (dependencies.Length > 0)
//                {
//                    m_Dependencies.Add(abName, dependencies);
//                    for (int i = 0; i < dependencies.Length; i++)
//                    {
//                        string depName = dependencies[i];
//                        AssetBundleInfo bundleInfo = null;
//                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
//                        {
//                            bundleInfo.m_ReferencedCount++;
//                        }
//                        else if (!m_LoadRequests.ContainsKey(depName) && !m_loadingDependencies.Contains(depName))
//                        {
//                            //这里加了这个判断,为了处理多个bundle同时加载且同时有依赖一个资源的情况
//                            m_loadingDependencies.Add(depName);
//                            yield return _mono.StartCoroutine(OnLoadAssetBundle(depName, type));
//                            bundleInfo = GetLoadedAssetBundle(depName);
//                            if (bundleInfo != null)
//                                bundleInfo.m_ReferencedCount++;
//                            //Debug.Log(depName + " ReferencedCount++ " + bundleInfo.m_ReferencedCount);
//                        }
//                        //如果这个依赖包正在加载
//                        //必须等待,直到这个依赖包加载完毕
//                        while (m_loadingDependencies.Contains(depName))
//                        {
//                            yield return null;
//                        }
//                    }
//                }
//                download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
//            }
//            yield return download;

//            if (m_loadingDependencies.Contains(abName))
//            {
//                m_loadingDependencies.Remove(abName);
//            }

//            AssetBundle assetObj = download.assetBundle;
//            if (assetObj != null)
//            {
//                m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
//            }

//        }

//        AssetBundleInfo GetLoadedAssetBundle(string abName)
//        {
//            AssetBundleInfo bundle = null;
//            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
//            if (bundle == null) return null;

//            // No dependencies are recorded, only the bundle itself is required.
//            string[] dependencies = null;
//            if (!m_Dependencies.TryGetValue(abName, out dependencies))
//                return bundle;

//            // Make sure all dependencies are loaded
//            foreach (var dependency in dependencies)
//            {
//                AssetBundleInfo dependentBundle;
//                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
//                if (dependentBundle == null) return null;
//            }
//            return bundle;
//        }

//        /// <summary>
//        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
//        /// </summary>
//        /// <param name="abName"></param>
//        /// <param name="isThorough"></param>
//        public void UnloadAssetBundle(string abName, bool isThorough = false)
//        {
//            abName = GetRealAssetPath(abName);
//            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
//            if (UnloadAssetBundleInternal(abName, isThorough))
//                UnloadDependencies(abName, isThorough);
//            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
//        }

//        void UnloadDependencies(string abName, bool isThorough)
//        {
//            string[] dependencies = null;
//            if (!m_Dependencies.TryGetValue(abName, out dependencies))
//                return;

//            // Loop dependencies.
//            foreach (var dependency in dependencies)
//            {
//                UnloadAssetBundleInternal(dependency, isThorough);
//            }
//            m_Dependencies.Remove(abName);
//        }

//        bool UnloadAssetBundleInternal(string abName, bool isThorough)
//        {
//            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
//            if (bundle == null) return true;

//            if (--bundle.m_ReferencedCount <= 0)
//            {
//                if (m_LoadRequests.ContainsKey(abName))
//                {
//                    return true;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
//                }
//                bundle.m_AssetBundle.Unload(isThorough);
//                m_LoadedAssetBundles.Remove(abName);
//                Debug.Log(abName + " has been unloaded successfully");
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        ///////////////////同步加载////////////////////////////
//        public T LoadAssetSync<T>(string abname, string assetname) where T : UnityEngine.Object
//        {
//            abname = abname.ToLower();
//            AssetBundle bundle = LoadAssetBundleSync(abname);
//            T asset = bundle.LoadAsset<T>(assetname);
//            return asset;
//        }

//        public void UnloadSyncAsset()
//        {
//            foreach (AssetBundle bundle in syncBundles.Values)
//            {
//                bundle.Unload(false);
//            }
//            syncBundles.Clear();
//        }

//        public AssetBundle LoadAssetBundleSync(string abname)
//        {
//            //Debug.Log("LoadAssetBundleSync abname:" + abname);
//            if (!abname.EndsWith(AppConst.ExtName))
//            {
//                abname += AppConst.ExtName;
//            }
//            AssetBundle bundle = null;

//            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abname);
//            if (bundleInfo == null)
//            {
//                if (!syncBundles.ContainsKey(abname))
//                {
//                    byte[] stream = null;
//                    string uri = Util.DataPath + abname;
//                    //Debuger.LogWarning("LoadFile::>> " + uri);
//                    LoadDependenciesSync(abname);

//                    stream = File.ReadAllBytes(uri);
//                    bundle = AssetBundle.LoadFromMemory(stream); //关联数据的素材绑定
//                    syncBundles.Add(abname, bundle);
//                }
//                else
//                {
//                    syncBundles.TryGetValue(abname, out bundle);
//                }
//            }
//            else
//            {
//                bundle = bundleInfo.m_AssetBundle;
//                //bundleInfo.m_ReferencedCount++;
//            }

//            return bundle;
//        }

//        void LoadDependenciesSync(string name)
//        {
//            if (manifest == null)
//            {
//                //Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
//                return;
//            }
//            // Get dependecies from the AssetBundleManifest object..
//            string[] dependencies = manifest.GetAllDependencies(name);
//            if (dependencies.Length == 0) return;

//            for (int i = 0; i < dependencies.Length; i++)
//                dependencies[i] = RemapVariantNameSync(dependencies[i]);

//            // Record and load all dependencies.
//            for (int i = 0; i < dependencies.Length; i++)
//            {
//                LoadAssetBundleSync(dependencies[i]);
//            }
//        }

//        string RemapVariantNameSync(string assetBundleName)
//        {
//            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

//            // If the asset bundle doesn't have variant, simply return.
//            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
//                return assetBundleName;

//            string[] split = assetBundleName.Split('.');

//            int bestFit = int.MaxValue;
//            int bestFitIndex = -1;
//            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
//            for (int i = 0; i < bundlesWithVariant.Length; i++)
//            {
//                string[] curSplit = bundlesWithVariant[i].Split('.');
//                if (curSplit[0] != split[0])
//                    continue;

//                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
//                if (found != -1 && found < bestFit)
//                {
//                    bestFit = found;
//                    bestFitIndex = i;
//                }
//            }
//            if (bestFitIndex != -1)
//                return bundlesWithVariant[bestFitIndex];
//            else
//                return assetBundleName;
//        }

//        void OnDestroy()
//        {
//            if (shared != null) shared.Unload(true);
//            if (manifest != null) manifest = null;
//            // Debuger.Log("~ResourceManager was destroy!");
//        }
//    }
//}