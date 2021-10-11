using Loxodon.Framework.Security.Cryptography;
using System;

namespace Loxodon.Framework.Bundles.Editors
{
    public class CryptographBundleModifier : IBundleModifier
    {
        private IStreamEncryptor encryptor;
        private Func<BundleInfo, bool> filter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptor"></param>
        public CryptographBundleModifier(IStreamEncryptor encryptor) : this(encryptor, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptor"></param>
        /// <param name="filter"></param>
        public CryptographBundleModifier(IStreamEncryptor encryptor, Func<BundleInfo, bool> filter)
        {
            this.filter = filter;
            this.encryptor = encryptor;
        }

        public virtual void Modify(BundleData bundleData)
        {
            BundleInfo bundleInfo = bundleData.BundleInfo;
            if (filter != null && !filter(bundleInfo))
                return;

            var data = this.encryptor.Encrypt(bundleData.Data);
            bundleData.Data = data;
            bundleInfo.FileSize = data.Length;
            bundleInfo.Encoding = this.encryptor.AlgorithmName;
        }
    }
}
