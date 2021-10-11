using Loxodon.Framework.Asynchronous;
using System.Collections.Generic;

namespace Loxodon.Framework.Bundles
{
    /// <summary>
    /// A common interface for bundle manager.
    /// </summary>
    public interface IBundleManager
    {
        /// <summary>
        /// Gets a bundle for the given bundle's name.If the Assetbundle isn't loaded, returns null.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        IBundle GetBundle(string bundleName);
        
        /// <summary>
        /// Gets bundle asset names
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        string[] GetBundleAssetNames(string bundleName);

        /// <summary>
        /// 获得已加载的bundle名字列表
        /// </summary>
        string[] GetBundleNames();

        /// <summary>
        /// 获得所有定义的 bundle 名字列表，与是否已加载无关
        /// </summary>
        List<string> GetAllBundleNames();

        /// <summary>
        /// Asynchronously loads a bundle for the given bundle's name.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        IProgressResult<float, IBundle> LoadBundle(string bundleName);

        /// <summary>
        /// Asynchronously loads a bundle for the given bundle's name.
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="priority">Positive or negative, the default value is 0.When multiple asynchronous operations are queued up, the operation with the higher priority will be executed first. Once an operation has been started on the background thread, changing the priority will have no effect anymore.</param>
        /// <returns></returns>
        IProgressResult<float, IBundle> LoadBundle(string bundleName, int priority);

        /// <summary>
        /// Asynchronously loads a group of bundles for the given bundle's names.
        /// </summary>
        /// <param name="bundleNames"></param>
        /// <returns></returns>
        IProgressResult<float, IBundle[]> LoadBundle(params string[] bundleNames);

        /// <summary>
        /// Asynchronously loads a group of bundles for the given bundle's names.
        /// </summary>
        /// <param name="bundleNames"></param>
        /// <param name="priority">Positive or negative, the default value is 0.When multiple asynchronous operations are queued up, the operation with the higher priority will be executed first. Once an operation has been started on the background thread, changing the priority will have no effect anymore.</param>
        /// <returns></returns>
        IProgressResult<float, IBundle[]> LoadBundle(string[] bundleNames, int priority);

    }
}
