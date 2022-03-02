using System.Security.Cryptography;

namespace BluePrism.Core.Configuration
{
    public static class SecurityPolicy
    {
        public static bool EnforceFIPSCompliance()
        {
            return CryptoConfig.AllowOnlyFipsAlgorithms;
        }
    }
}