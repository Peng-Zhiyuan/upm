using System;
using System.Text;
using System.Security.Cryptography;

namespace Loxodon.Framework.Bundles.Editors
{
    public class HashFilenameBundleModifier : IBundleModifier
    {
        [ThreadStatic]
        private static MD5 md5;

        private static MD5 MD5
        {
            get
            {
                if (md5 == null)
                    md5 = new MD5CryptoServiceProvider();
                return md5;
            }
        }

        public virtual void Modify(BundleData bundleData)
        {
            BundleInfo bundleInfo = bundleData.BundleInfo;
            //string name = string.Format("{0}-{1}-{2}-{3}", bundleInfo.FullName, bundleInfo.Hash.ToString(), bundleInfo.CRC, bundleInfo.IsEncrypted ? bundleInfo.Encoding : "");

            // pzy:
            // 从取名因素中去掉了 CRC
            // 因为发现一个bug
            // 当来回切换分支后，可能会包内内容一样，bundle 尺寸相差数个字节，导致 CRC 不同
            // 这会触发对不必要的包进行更新
            string name = string.Format("{0}-{1}-{2}", bundleInfo.FullName, bundleInfo.Hash.ToString(), bundleInfo.IsEncrypted ? bundleInfo.Encoding : "");

            bundleInfo.Filename = BitConverter.ToString(MD5.ComputeHash(Encoding.ASCII.GetBytes(name))).Replace("-", "").ToLower();
        }
    }
}
