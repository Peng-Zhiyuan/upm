using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Loxodon.Framework.Bundles.Editors
{
    public class BundleBuilder
    {
        private const string GenOutputPath = "gen";
        private const string ManifestPrefix = "bundles";

        public BundleBuilder()
        {
        }

        protected virtual string RemoveExtensionAndHash(string name)
        {
            return Regex.Replace(name, @"(_[0-9a-fA-F]{32})|(\.[a-z0-9A-Z-_]+$)", "");
        }

        protected virtual List<string> GetLevelsFromBuildSettings()
        {
            List<string> levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    levels.Add(scene.path);
            }
            return levels;
        }

        public virtual List<FileInfo> FindBundleManifestFiles(string outputPath, BuildTarget buildTarget)
        {
            List<FileInfo> files = new List<FileInfo>();
            string platformOutput = System.IO.Path.Combine(outputPath, buildTarget.ToString()).Replace(@"\", "/");
            if (!Directory.Exists(platformOutput))
                return files;

            DirectoryInfo dir = new DirectoryInfo(platformOutput);
            foreach (FileInfo info in dir.GetFiles(BundleSetting.ManifestFilename, SearchOption.AllDirectories))
            {
                if (info.FullName.Replace(@"\", "/").EndsWith(string.Format("{0}/{1}", GenOutputPath, BundleSetting.ManifestFilename)))
                    continue;

                files.Add(info);
            }

            files.Sort((x, y) => y.FullName.CompareTo(x.FullName));
            return files;
        }

        public virtual FileInfo GetLatestBundleManifestFile(string outputPath, BuildTarget buildTarget)
        {
            List<FileInfo> files = this.FindBundleManifestFiles(outputPath, buildTarget);
            if (files.Count == 0)
                return null;
            return files[0];
        }

        public virtual FileInfo GetPreviousBundleManifestFile(string outputPath, BuildTarget buildTarget, string version)
        {
            List<FileInfo> files = this.FindBundleManifestFiles(outputPath, buildTarget);
            if (files.Count == 0)
                return null;

            foreach (FileInfo info in files)
            {
                var _version = info.Directory.Name;
                if (_version.CompareTo(version) >= 0)
                    continue;

                return info;
            }
            return null;
        }

        public virtual List<BundleManifest> FindBundleManifests(string outputPath, BuildTarget buildTarget)
        {
            List<BundleManifest> bundles = new List<BundleManifest>();
            List<FileInfo> files = this.FindBundleManifestFiles(outputPath, buildTarget);
            if (files.Count == 0)
                return bundles;

            for (int i = 0; i < files.Count; i++)
            {
                var info = files[i];
                var json = File.ReadAllText(info.FullName);
                bundles.Add(BundleManifest.Parse(json));
            }
            return bundles;
        }

        public virtual BundleManifest GetLatestBundleManifest(string outputPath, BuildTarget buildTarget)
        {
            FileInfo file = this.GetLatestBundleManifestFile(outputPath, buildTarget);
            if (file == null)
                return null;
            var json = File.ReadAllText(file.FullName);
            return BundleManifest.Parse(json);
        }

        public virtual BundleManifest GetPreviousBundleManifest(string outputPath, BuildTarget buildTarget, string version)
        {
            FileInfo file = this.GetPreviousBundleManifestFile(outputPath, buildTarget, version);
            if (file == null)
                return null;
            var json = File.ReadAllText(file.FullName);
            return BundleManifest.Parse(json);
        }

        public virtual string GetPlatformOutput(string outputPath, BuildTarget buildTarget)
        {
            return System.IO.Path.Combine(outputPath, buildTarget.ToString()).Replace(@"\", "/");
        }

        public virtual string GetGenOutput(string outputPath, BuildTarget buildTarget)
        {
            string platformOutput = GetPlatformOutput(outputPath, buildTarget);
            return platformOutput + "/" + GenOutputPath;
        }

        public virtual string GetVersionOutput(string outputPath, BuildTarget buildTarget, string version)
        {
            string platformOutput = GetPlatformOutput(outputPath, buildTarget);
            return platformOutput + "/" + version;
        }

        public virtual BundleManifest CopyAssetBundleAndManifest(DirectoryInfo src, DirectoryInfo dest, List<IBundleModifier> bundleModifierChain = null, IBundleFilter bundleFilter = null)
        {
            if (!src.Exists)
                throw new DirectoryNotFoundException(string.Format("Not found the directory '{0}'.", src.FullName));

            try
            {
                string json = File.ReadAllText(System.IO.Path.Combine(src.FullName, BundleSetting.ManifestFilename).Replace(@"\", "/"));
                BundleManifest manifest = BundleManifest.Parse(json);
                manifest = this.CopyAssetBundle(manifest, src, dest, bundleModifierChain, bundleFilter);
                if (manifest != null)
                    File.WriteAllText(System.IO.Path.Combine(dest.FullName, BundleSetting.ManifestFilename).Replace(@"\", "/"), manifest.ToJson());
                return manifest;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(string.Format("Copy AssetBundles failure from {0} to {1}.", src.FullName, dest.FullName), e);
            }
        }

        public virtual BundleManifest CopyAssetBundle(BundleManifest manifest, DirectoryInfo src, DirectoryInfo dest, List<IBundleModifier> bundleModifierChain = null, IBundleFilter bundleFilter = null)
        {
            if (!src.Exists)
                throw new DirectoryNotFoundException(string.Format("Not found the directory '{0}'.", src.FullName));

            try
            {
                foreach (BundleInfo bundleInfo in manifest.GetAll())
                {
                    if (bundleFilter != null && !bundleFilter.IsValid(bundleInfo))
                        continue;

                    FileInfo srcFile = new FileInfo(System.IO.Path.Combine(src.FullName, bundleInfo.Filename).Replace(@"\", "/"));
                    byte[] data = File.ReadAllBytes(srcFile.FullName);
                    BundleData bundleData = new BundleData(bundleInfo, data);
                    if (bundleModifierChain != null && bundleModifierChain.Count > 0)
                    {
                        foreach (IBundleModifier modifier in bundleModifierChain)
                        {
                            modifier.Modify(bundleData);
                        }
                    }

                    FileInfo destFile = new FileInfo(System.IO.Path.Combine(dest.FullName, bundleInfo.Filename).Replace(@"\", "/"));
                    if (destFile.Exists)
                        destFile.Delete();

                    if (!destFile.Directory.Exists)
                        destFile.Directory.Create();

                    File.WriteAllBytes(destFile.FullName, bundleData.Data);
                }
                return manifest;
            }
            catch (System.Exception e)
            {
                throw new System.Exception(string.Format("Copy AssetBundles failure from {0} to {1}.", src.FullName, dest.FullName), e);
            }
        }

        public virtual List<BundleInfo> GetDeltaUpdates(BundleManifest previousVersion, BundleManifest currentVersion, bool compareCRC = false)
        {
            List<BundleInfo> bundles = new List<BundleInfo>();

            Dictionary<string, BundleInfo> dict = new Dictionary<string, BundleInfo>();
            foreach (BundleInfo bundle in previousVersion.GetAll())
                dict.Add(bundle.FullName, bundle);

            foreach (BundleInfo bundle in currentVersion.GetAll())
            {
                BundleInfo previous;
                if (!dict.TryGetValue(bundle.FullName, out previous))
                {
                    bundles.Add(bundle);
                    continue;
                }

                if (previous.Hash.Equals(bundle.Hash) && previous.Encoding.Equals(bundle.Encoding) && (!compareCRC || previous.CRC == bundle.CRC))
                    continue;

                bundles.Add(bundle);
            }
            return bundles;
        }

        public virtual void Build(string outputPath, BuildTarget buildTarget, BuildAssetBundleOptions options, string version, List<IBundleModifier> bundleModifierChain = null)
        {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                    throw new System.Exception("Switch BuildTarget failure.");
            }

            if (string.IsNullOrEmpty(outputPath))
                throw new System.ArgumentNullException("outputPath");

            if (string.IsNullOrEmpty(version))
                throw new System.ArgumentNullException("version");


            string platformOutput = this.GetPlatformOutput(outputPath, buildTarget);
            string genOutput = this.GetGenOutput(outputPath, buildTarget);
            string versionOutput = this.GetVersionOutput(outputPath, buildTarget, version);

            if (((options & BuildAssetBundleOptions.ForceRebuildAssetBundle) > 0) && Directory.Exists(genOutput))
                Directory.Delete(genOutput, true);

            if (Directory.Exists(versionOutput))
                Directory.Delete(versionOutput, true);

            if (!Directory.Exists(genOutput))
                Directory.CreateDirectory(genOutput);

            if (!Directory.Exists(versionOutput))
                Directory.CreateDirectory(versionOutput);

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(genOutput, options, buildTarget);
            if (manifest == null)
                throw new System.Exception("Build failure.");

#if UNITY_5_6_OR_NEWER
            if ((options & BuildAssetBundleOptions.DryRunBuild) > 0)
                return;
#endif

            try
            {
                AssetDatabase.StartAssetEditing();

                BundleManifest lastVersionBundleManifest = this.GetPreviousBundleManifest(outputPath, buildTarget, version);
                List<BundleInfo> lastBundles = new List<BundleInfo>();
                if (lastVersionBundleManifest != null)
                    lastBundles.AddRange(lastVersionBundleManifest.GetAll());

                BundleManifest bundleManifest = this.CreateBundleManifest(genOutput, manifest, version);

                bundleManifest = this.CopyAssetBundle(bundleManifest, new DirectoryInfo(genOutput), new DirectoryInfo(versionOutput), bundleModifierChain);
                File.WriteAllText(string.Format("{0}/{1}", versionOutput, BundleSetting.ManifestFilename), bundleManifest.ToJson());
                File.WriteAllText(string.Format("{0}/{1}_{2}.csv", platformOutput, ManifestPrefix, version), this.ToCSV(lastBundles.ToArray(), bundleManifest.GetAll()));
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }


        /// <summary>
        /// build player.
        /// </summary>
        /// <param name="buildTarget">Build target.</param>
        /// <param name="pathInfo">Location.</param>
        /// <param name="levels">Levels.</param>
        public virtual void BuildPlayer(BuildTarget buildTarget, string pathInfo, string[] levels = null)
        {
            if (levels == null || levels.Length == 0)
                levels = GetLevelsFromBuildSettings().ToArray();

            BuildPipeline.BuildPlayer(levels, pathInfo, buildTarget, BuildOptions.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolder">The AssetBundle directory.</param>
        /// <param name="version"></param>
        /// <param name="defaultVariant"></param>
        /// <returns></returns>
        public virtual BundleManifest CreateBundleManifest(string rootFolder, string version, string defaultVariant = null)
        {
            DirectoryInfo root = new DirectoryInfo(rootFolder);
            if (!root.Exists)
                throw new DirectoryNotFoundException();

            AssetBundle bundle = AssetBundle.LoadFromFile(string.Format("{0}/{1}", root.FullName, root.Name));
            AssetBundleManifest manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            bundle.Unload(false);

            return this.CreateBundleManifest(rootFolder, manifest, version, defaultVariant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootFolder">The AssetBundle directory.</param>
        /// <param name="manifest"></param>
        /// <param name="version"></param>
        /// <param name="defaultVariant"></param>
        /// <returns></returns>
        public virtual BundleManifest CreateBundleManifest(string rootFolder, AssetBundleManifest manifest, string version, string defaultVariant = null)
        {
            if (manifest == null)
                throw new System.ArgumentNullException("manifest");

            List<BundleInfo> bundles = new List<BundleInfo>(); /* Bundle list */
            foreach (string bundleFullName in manifest.GetAllAssetBundles())
            {
                BundleInfo bundle = CreateBundleInfo(rootFolder, bundleFullName, manifest);
                if (bundle == null)
                    continue;

                bundles.Add(bundle);
            }

            BundleManifest bundleManifest = new BundleManifest(bundles, version, null);
            this.CheckAssetBundle(bundleManifest);
            return bundleManifest;
        }

        /// <summary>
        /// Create BundleInfo
        /// </summary>
        /// <param name="rootFolder">The AssetBundle directory.</param>
        /// <param name="filename">The filename</param>
        /// <param name="manifest"></param>
        /// <returns></returns>
        protected virtual BundleInfo CreateBundleInfo(string rootFolder, string filename, AssetBundleManifest manifest)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            string path = rootFolder + "/" + filename;
            FileInfo file = new FileInfo(path);
            string bundleName = this.RemoveExtensionAndHash(filename);
            long size = file.Length;
            uint crc = 0;
            BuildPipeline.GetCRCForAssetBundle(file.FullName, out crc);
            Hash128 hash = manifest.GetAssetBundleHash(filename);
            string variant = file.Extension.Replace(".", "");
            string[] dependencies = manifest.GetDirectDependencies(filename);
            string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(string.IsNullOrEmpty(variant) ? bundleName : string.Format("{0}.{1}", bundleName, variant));

            // 过滤 .cs 文件
            assets = FilteCsFile(assets);

            return new BundleInfo(bundleName, variant, hash, crc, size, filename, true, assets, dependencies);
        }

        string[] FilteCsFile(string[] pathList)
        {
            var ret = new List<string>();

            // 过滤.cs 文件
            foreach (var path in pathList)
            {
                if(path.EndsWith(".cs"))
                {
                    continue;
                }
                ret.Add(path);
            }
            var list = ret.ToArray();
            return list;
        }

        /// <summary>
        /// 转为CSV文件格式.
        /// </summary>
        /// <returns>The bundleInfo to CSV.</returns>
        /// <param name="bundleInfos">List.</param>
        protected virtual string ToCSV(BundleInfo[] bundleInfos)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("\"Name\"").Append(",");
            buf.Append("\"HASH\"").Append(",");
            buf.Append("\"CRC\"").Append(",");
            buf.Append("\"Size\"").Append(",");
            buf.Append("\"Encoding\"").Append(",");
            buf.Append("\"Published\"").Append(",");
            buf.Append("\"Filename\"").Append("\r\n");

            foreach (BundleInfo bundle in bundleInfos)
            {
                buf.Append("\"").Append(bundle.FullName).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Hash.ToString()).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.CRC).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.FileSize).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Encoding).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Published).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Filename).Append("\"").Append("\r\n");
            }
            return buf.ToString();
        }

        protected virtual string ToCSV(BundleInfo[] previousVersionBundleInfos, BundleInfo[] currVersionBundleInfos)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("\"Name\"").Append(",");
            buf.Append("\"HASH\"").Append(",");
            buf.Append("\"CRC\"").Append(",");
            buf.Append("\"Size\"").Append(",");
            buf.Append("\"Encoding\"").Append(",");
            buf.Append("\"Published\"").Append(",");
            buf.Append("\"Filename\"").Append(",");
            buf.Append("\"State\"").Append("\r\n");

            Dictionary<string, BundleInfoPair> dict = new Dictionary<string, BundleInfoPair>();
            foreach (BundleInfo bundle in previousVersionBundleInfos)
            {
                dict.Add(bundle.FullName, new BundleInfoPair(bundle, null));
            }

            foreach (BundleInfo bundle in currVersionBundleInfos)
            {
                BundleInfoPair pair;
                if (!dict.TryGetValue(bundle.FullName, out pair))
                {
                    dict.Add(bundle.FullName, new BundleInfoPair(null, bundle));
                    continue;
                }

                pair.BundleInfo2 = bundle;
            }

            List<BundleInfoPair> bundles = new List<BundleInfoPair>();
            bundles.AddRange(dict.Values);
            bundles.Sort((x, y) => x.Name.CompareTo(y.Name));

            long totalSize = 0L;
            long updatedSize = 0L;
            long deletedSize = 0L;

            foreach (BundleInfoPair pair in bundles)
            {
                BundleInfo bundle = pair.BundleInfo2 != null ? pair.BundleInfo2 : pair.BundleInfo1;
                buf.Append("\"").Append(bundle.FullName).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Hash.ToString()).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.CRC).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.FileSize).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Encoding).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Published).Append("\"").Append(",");
                buf.Append("\"").Append(bundle.Filename).Append("\"").Append(",");
                buf.Append("\"").Append(pair.State).Append("\"").Append("\r\n");

                if (pair.State != BundleState.DELETED)
                    totalSize += pair.BundleInfo2.FileSize;

                if (pair.State == BundleState.DELETED)
                    deletedSize += pair.BundleInfo1.FileSize;

                if (pair.State == BundleState.ADDED || pair.State == BundleState.CHANGED)
                    updatedSize += pair.BundleInfo2.FileSize;
            }

            buf.Append("\r\n");

            buf.Append("\"").Append("Total Size").Append("\"").Append(",");
            buf.Append("\"").Append("Updated Size").Append("\"").Append(",");
            buf.Append("\"").Append("Deleted Size").Append("\"").Append("\r\n");

            buf.Append("\"").Append(totalSize / (float)1048576).Append(" MB\"").Append(",");
            buf.Append("\"").Append(updatedSize / (float)1048576).Append(" MB\"").Append(",");
            buf.Append("\"").Append(deletedSize / (float)1048576).Append(" MB\"").Append("\r\n");
            return buf.ToString();
        }

        /// <summary>
        /// Check the loop reference.
        /// </summary>
        /// <param name="bundleManifest">Bundle manifest.</param>
        protected virtual void CheckAssetBundle(BundleManifest bundleManifest)
        {
            BundleInfo[] bundles = bundleManifest.GetAll();
            foreach (BundleInfo bundle in bundles)
            {
                bundleManifest.GetDependencies(bundle.Name, true);
            }
        }

        internal enum BundleState
        {
            DELETED,
            ADDED,
            CHANGED,
            UNCHANGED
        }

        internal class BundleInfoPair
        {
            private BundleInfo bundleInfo1;
            private BundleInfo bundleInfo2;
            public BundleInfoPair(BundleInfo bundleInfo1, BundleInfo bundleInfo2)
            {
                this.bundleInfo1 = bundleInfo1;
                this.bundleInfo2 = bundleInfo2;
            }

            public string Name
            {
                get
                {
                    if (bundleInfo1 != null)
                        return this.bundleInfo1.FullName;
                    if (bundleInfo2 != null)
                        return this.bundleInfo2.FullName;
                    return string.Empty;
                }
            }

            public BundleState State
            {
                get
                {
                    if (this.bundleInfo1 != null && this.bundleInfo2 == null)
                        return BundleState.DELETED;

                    if (this.bundleInfo1 == null && this.bundleInfo2 != null)
                        return BundleState.ADDED;

                    if (this.bundleInfo1.Hash.Equals(this.bundleInfo2.Hash) && this.bundleInfo1.Encoding == this.bundleInfo2.Encoding && this.bundleInfo1.CRC == this.bundleInfo2.CRC)
                        return BundleState.UNCHANGED;

                    return BundleState.CHANGED;
                }
            }

            public BundleInfo BundleInfo1
            {
                get { return bundleInfo1; }
                set { this.bundleInfo1 = value; }
            }

            public BundleInfo BundleInfo2
            {
                get { return bundleInfo2; }
                set { this.bundleInfo2 = value; }
            }
        }
    }
}
