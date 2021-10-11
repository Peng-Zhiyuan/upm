using System;
using System.IO;

namespace Loxodon.Framework.Bundles
{
    [Obsolete("Please use \"Loxodon.Framework.Security.Cryptography.IDecryptor\" instead of this class.")]
    public interface IDecryptor : Security.Cryptography.IStreamDecryptor
    {
    }
}
