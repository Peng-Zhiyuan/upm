using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Loxodon.Framework.Bundles
{
    public class AutoMappingPathInfoParser : IPathInfoParser, IManifestUpdatable
    {
        private BundleManifest bundleManifest;

        // path to bundle name
        private Dictionary<string, string> pathToBundleNameDic = new Dictionary<string, string>();

        public AutoMappingPathInfoParser(BundleManifest manifest)
        {
            this.BundleManifest = manifest;
        }

        public virtual BundleManifest BundleManifest
        {
            get { return this.bundleManifest; }
            set
            {
                if (this.bundleManifest == value)
                    return;

                this.bundleManifest = value;
                this.Initialize();
            }
        }

        protected virtual void Initialize()
        {
            if (this.pathToBundleNameDic != null)
                this.pathToBundleNameDic.Clear();

            if (this.pathToBundleNameDic == null)
                this.pathToBundleNameDic = new Dictionary<string, string>();

            Regex regex = new Regex("^assets/", RegexOptions.IgnoreCase);
            BundleInfo[] bundleInfos = this.bundleManifest.GetAll();
            foreach (BundleInfo bndeInfo in bundleInfos)
            {
                if (!bndeInfo.Published)
                    continue;

                var assets = bndeInfo.Assets;
                var bundleName = bndeInfo.Name;

                //if(bundleName == "bundles/music")
                //{
                //    Debug.Log("log");
                //}

                for (int i = 0; i < assets.Length; i++)
                {
                    var assetPath = assets[i].ToLower();
                    var key = regex.Replace(assetPath, "");
                    pathToBundleNameDic[key] = bundleName;
                    //Debug.Log($"{key} -> {info.Name}");
                    //if (key.StartsWith("game/res/code"))
                    //{
                    //    Debug.Log($"{key} -> {bndeInfo.Name}");
                    //}
                }
            }
        }

        public virtual AssetPathInfo Parse(string path)
        {
            string bundleName;
            if (!this.pathToBundleNameDic.TryGetValue(path.ToLower(), out bundleName))
                return null;

            return new AssetPathInfo(bundleName, path);
        }
    }
}
