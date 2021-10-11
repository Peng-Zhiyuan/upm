using System;
using System.IO;

namespace Loxodon.Framework.Bundles
{
    [Obsolete("Please use \"Loxodon.Framework.Security.Cryptography.IEncryptor\" instead of this class.")]
    public interface IEncryptor : Security.Cryptography.IStreamEncryptor
    {
    }
}
