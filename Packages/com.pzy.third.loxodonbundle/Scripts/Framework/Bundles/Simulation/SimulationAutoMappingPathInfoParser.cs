#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;

using Loxodon.Log;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Loxodon.Framework.Bundles
{
    public class SimulationAutoMappingPathInfoParser : IPathInfoParser
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // assetPath to bundleName
        // like:
        // bundles/_atlas/fight.prefab -> bundles/atlas
        private Dictionary<string, string> dict = new Dictionary<string, string>();

        public SimulationAutoMappingPathInfoParser()
        {
            this.Initialize();
        }

        protected virtual void Initialize()
        {
            Regex regex = new Regex("^assets/", RegexOptions.IgnoreCase);
            foreach (string bundleName in AssetDatabaseHelper.GetUsedAssetBundleNames())
            {
                var assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                for (int i = 0; i < assets.Length; i++)
                {
                    //Debug.Log("assets: " + assets[i]);
                    var assetPath = assets[i].ToLower();      
                    //Debug.Log("assets2: " + assetPath);             
                    var key = regex.Replace(assetPath, "");
                    //Debug.Log("key: " + key);        
                    dict[key] = Path.GetFilePathWithoutExtension(bundleName).ToLower();
//                    Debug.Log($"{key} -> {dict[key]}");
                }
            }
        }

        public virtual AssetPathInfo Parse(string path)
        {
            string bundleName;
            if (!this.dict.TryGetValue(path.ToLower(), out bundleName))
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("Not found the AssetBundle,please check the configuration of the asset '{0}'.", path);
                return null;
            }

            return new AssetPathInfo(bundleName, path);
        }
    }
}
#endif