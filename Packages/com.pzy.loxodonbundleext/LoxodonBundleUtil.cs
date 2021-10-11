using System;
using System.Text;
using UnityEngine;

using Loxodon.Framework.Bundles;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Security.Cryptography;
using Loxodon.Framework.Examples.Bundle;
using System.IO;

public static class LoxodonBundleUtil
{
    private static string iv = "5Hh2390dQlVh0AqC";
    private static string key = "E4YZgiGQ0aqe5LEJ";

    private static bool created = false;
    public static void CreateBundleService(LoxodonBundleResolution resuolution)
    {           
        if(created)
        {
            return;
        }
        var resources = CreateResources(resuolution);
        var context = Context.GetApplicationContext();
        context.GetContainer().Register<IResources>(resources);
        created = true;
    }

    private static IResources CreateResources(LoxodonBundleResolution resolution)
    {
        if(resolution == LoxodonBundleResolution.Simulation)
        {
            var ret = CreateResourcesBySimulation();
            return ret; 
        }
        else if(resolution == LoxodonBundleResolution.Embeded)
        {
            var ret = CreateResourcesByEmbeded();
            return ret;   
        }
        else if(resolution == LoxodonBundleResolution.Remote)
        {
            var ret = CreateResourcesByRemote();
            return ret;
        }
        else
        {
            throw new Exception("unsupport BundleResolution: " + resolution);
        }
    }

    private static IResources CreateResourcesBySimulation()
    {
        // 模拟
        // 这个方式仅在编辑器中工作

        #if UNITY_EDITOR
        Debug.Log("[BundleResolutionManager] Use SimulationResources.");

        /* Create a PathInfoParser. */
        //IPathInfoParser pathInfoParser = new SimplePathInfoParser("@");
        IPathInfoParser pathInfoParser = new SimulationAutoMappingPathInfoParser();

        /* Create a BundleManager */
        IBundleManager manager = new SimulationBundleManager();

        /* Create a BundleResources */
        var resources = new SimulationResources(pathInfoParser, manager);

        return resources;
        #else 
        throw new Exception("BundleResolution: Simulation not works on current player");
        #endif
    }

    private static IResources CreateResourcesByEmbeded()
    {
        // 内置
        // 只使用包内内嵌的 bundle
        Debug.Log("[BundleResolutionManager] Use Embeded.");

        /* Create a BundleManifestLoader. */
        //var manifestLoader = new BundleManifestLoader();
        ///* Loads BundleManifest. */
        //var readonlyDirectory = BundleUtil.GetReadOnlyDirectory();
        //var path = readonlyDirectory + BundleSetting.ManifestFilename;
        //var manifest = manifestLoader.Load(path);

        var manifest = LoadEmbededBundleManifest();
        if(manifest == null)
        {
            throw new Exception("no embeded res package found while bundle resolution is embleded");
        }

        //manifest.ActiveVariants = new string[] { "", "sd" };
        manifest.ActiveVariants = new string[] { "", "hd" };

        //Debug.Log("manifest: " + manifest);

        /* Create a PathInfoParser. */
        //IPathInfoParser pathInfoParser = new SimplePathInfoParser("@");
        var pathInfoParser = new AutoMappingPathInfoParser(manifest);

        //Debug.Log("pathInfoParser: " + pathInfoParser);

        /* Create a BundleLoaderBuilder */
        //ILoaderBuilder builder = new WWWBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false);

        /* AES128_CBC_PKCS7 */
        //RijndaelCryptograph rijndaelCryptograph = new RijndaelCryptograph(128, Encoding.ASCII.GetBytes(this.key), Encoding.ASCII.GetBytes(this.iv));
        var decryptor = CryptographUtil.GetDecryptor(Algorithm.AES128_CBC_PKCS7, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(iv));

        /* Use a custom BundleLoaderBuilder */
        var builder = new CustomBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false, decryptor);

        /* Create a BundleManager */
        var manager = new BundleManager(manifest, builder);

        /* Create a BundleResources */
        var resources = new BundleResources(pathInfoParser, manager);

        return resources;
    }

    private static IResources CreateResourcesByRemote()
    {
        Debug.Log("[BundleResolutionManager] Use Remote.");
        
        /* Create a BundleManifestLoader. */
        //var manifestLoader = new BundleManifestLoader();

        // load embeded bundle manifest
        BundleManifest embededManifest = LoadEmbededBundleManifest();

        // load storaged bundle manifest
        BundleManifest storagedManifest = LoadStoragedBundleManifest();

        /* Loads BundleManifest. */
        BundleManifest manifest = null;
        if(storagedManifest != null)
        {
            manifest = storagedManifest;
        }
        else
        {
            manifest = embededManifest;
        }

        //manifest.ActiveVariants = new string[] { "", "sd" };
        manifest.ActiveVariants = new string[] { "", "hd" };

        /* Create a PathInfoParser. */
        //IPathInfoParser pathInfoParser = new SimplePathInfoParser("@");
        var pathInfoParser = new AutoMappingPathInfoParser(manifest);

        /* Create a BundleLoaderBuilder */
        //ILoaderBuilder builder = new WWWBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false);

        /* AES128_CBC_PKCS7 */
        //RijndaelCryptograph rijndaelCryptograph = new RijndaelCryptograph(128, Encoding.ASCII.GetBytes(this.key), Encoding.ASCII.GetBytes(this.iv));
        var decryptor = CryptographUtil.GetDecryptor(Algorithm.AES128_CBC_PKCS7, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(iv));

        /* Use a custom BundleLoaderBuilder */
        var builder = new CustomBundleLoaderBuilder(new Uri(BundleUtil.GetReadOnlyDirectory()), false, decryptor);

        /* Create a BundleManager */
        var manager = new BundleManager(manifest, builder);

        /* Create a BundleResources */
        var resources = new BundleResources(pathInfoParser, manager);

        return resources;
    }

    /// <summary>
    /// 文件不存在时返回 null
    /// </summary>
    /// <returns></returns>
    public static BundleManifest LoadEmbededBundleManifest()
    {
        /* Create a BundleManifestLoader. */
        var manifestLoader = new BundleManifestLoader();

        // load embeded bundle manifest
        BundleManifest embededManifest = null;
        {
            var readonlyDirectory = BundleUtil.GetReadOnlyDirectory();
            var path = readonlyDirectory + BundleSetting.ManifestFilename;
            Debug.Log("[BundleResolutionManager] embeded path: " + path);
            embededManifest = manifestLoader.Load(path);
        }
        return embededManifest;
    }

    /// <summary>
    /// 文件不存在时返回 null
    /// </summary>
    /// <returns></returns>
    public static BundleManifest LoadStoragedBundleManifest()
    {
        /* Create a BundleManifestLoader. */
        var manifestLoader = new BundleManifestLoader();

        // load embeded bundle manifest
        BundleManifest storagedManifest = null;
        {
            var storableDiretory = BundleUtil.GetStorableDirectory();
            var path = storableDiretory + BundleSetting.ManifestFilename;
            Debug.Log("[BundleResolutionManager] storage path: " + path);
            if (File.Exists(path))
            {
                storagedManifest = manifestLoader.Load(path);
            }
        }
        return storagedManifest;
    }

    public static IResources GetResoucesFromLoxodonContext()
    {
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        return resources;
    }


    public static string GetVersionFromCurrentBundleResurce()
    {
        var resources = GetResoucesFromLoxodonContext();
#if UNITY_EDITOR
        if (resources is SimulationResources)
        {
            // 这是编辑器中的模拟 bundle 模式, 没有资源版本号
            return "None";
        }
#endif
        if (resources is BundleResources)
        {
            // 这是实际工作时的模式
            var bundleResouces = resources as BundleResources;
            var version = bundleResouces.BundleManifest.Version;
            return version;
        }
        throw new Exception("unsuuprot resource type: " + resources.GetType().Name);
    }

    /// <summary>
    /// 如果下载和内置的资源都不存在，会返回 null
    /// </summary>
    /// <returns></returns>
    public static BundleManifest LoadLocalManifestInRemoteType()
    {
        var storageManifest = LoxodonBundleUtil.LoadStoragedBundleManifest();
        if (storageManifest != null)
        {
            return storageManifest;
        }
        var embededManifest = LoxodonBundleUtil.LoadEmbededBundleManifest();
        return embededManifest;
    }

    public static LoxodonBundleResolution Parse(string name)
    {
        if (name == "simulation")
        {
            return LoxodonBundleResolution.Simulation;
        }
        else if (name == "embeded")
        {
            return LoxodonBundleResolution.Embeded;
        }
        else if (name == "remote")
        {
            return LoxodonBundleResolution.Remote;
        }
        throw new Exception("unsupport bundle resolution name: " + name);
    }
}