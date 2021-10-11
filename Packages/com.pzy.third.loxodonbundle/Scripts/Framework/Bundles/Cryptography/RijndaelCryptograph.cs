using System;
using System.Security.Cryptography;

namespace Loxodon.Framework.Bundles
{
    [Obsolete("Please use \"Loxodon.Framework.Security.Cryptography.RijndaelCryptograph\" instead of this class.")]
    public class RijndaelCryptograph : Security.Cryptography.RijndaelCryptograph, IDecryptor, IEncryptor
    {
        private readonly static byte[] DEFAULT_IV = new byte[] { 45, 23, 12, 33, 44, 98, 67, 69, 22, 56, 22, 98, 99, 68, 75, 74 };
        private readonly static byte[] DEFAULT_KEY = new byte[] { 67, 69, 44, 98, 22, 12, 33, 12, 33, 44, 98, 67, 99, 68, 75, 74, 69, 22, 56, 22, 98, 98, 99, 68, 75, 74, 45, 23, 22, 56, 45, 23 };

        public RijndaelCryptograph() : this(256, DEFAULT_KEY, DEFAULT_IV)
        {
        }

        public RijndaelCryptograph(byte[] key, byte[] iv) : this(256, key, iv)
        {
        }

        public RijndaelCryptograph(int keySize, byte[] key, byte[] iv) : base(keySize, key, iv)
        {
        }

#if !NETFX_CORE
        public RijndaelCryptograph(RijndaelManaged rijndael, byte[] key, byte[] iv) : base(rijndael, key, iv)
        {
        }
#endif
    }
}
