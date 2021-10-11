using Loxodon.Framework.Attributes;
using Loxodon.Framework.Security.Cryptography;

namespace Loxodon.Framework.Bundles
{
    public enum Algorithm
    {
        [Remark("AES128 CBC PKCS7")]
        AES128_CBC_PKCS7 = 0,

        [Remark("AES192 CBC PKCS7")]
        AES192_CBC_PKCS7,

        [Remark("AES256 CBC PKCS7")]
        AES256_CBC_PKCS7,

#if UNITY_2018_1_OR_NEWER && !NETFX_CORE && !UNITY_WSA && !UNITY_WSA_10_0
        [Remark("AES128 CTR NONE")]
        AES128_CTR_NONE
#endif
    }

    public class CryptographUtil
    {
        public static IStreamEncryptor GetEncryptor(Algorithm algorithm, byte[] key, byte[] iv)
        {
            int keySize = 128;
            switch (algorithm)
            {
#if UNITY_2018_1_OR_NEWER && !NETFX_CORE && !UNITY_WSA && !UNITY_WSA_10_0
                case Algorithm.AES128_CTR_NONE:
                    return new AesCTRCryptograph(key, iv);
#endif
                case Algorithm.AES128_CBC_PKCS7:
                    keySize = 128;
                    break;
                case Algorithm.AES192_CBC_PKCS7:
                    keySize = 192;
                    break;
                case Algorithm.AES256_CBC_PKCS7:
                    keySize = 256;
                    break;
            }
            return new Security.Cryptography.RijndaelCryptograph(keySize, key, iv);
        }

        public static IStreamDecryptor GetDecryptor(Algorithm algorithm, byte[] key, byte[] iv)
        {
            int keySize = 128;
            switch (algorithm)
            {
#if UNITY_2018_1_OR_NEWER && !NETFX_CORE && !UNITY_WSA && !UNITY_WSA_10_0
                case Algorithm.AES128_CTR_NONE:
                    return new AesCTRCryptograph(key, iv);
#endif
                case Algorithm.AES128_CBC_PKCS7:
                    keySize = 128;
                    break;
                case Algorithm.AES192_CBC_PKCS7:
                    keySize = 192;
                    break;
                case Algorithm.AES256_CBC_PKCS7:
                    keySize = 256;
                    break;
            }
            return new Security.Cryptography.RijndaelCryptograph(keySize, key, iv);
        }
    }
}
