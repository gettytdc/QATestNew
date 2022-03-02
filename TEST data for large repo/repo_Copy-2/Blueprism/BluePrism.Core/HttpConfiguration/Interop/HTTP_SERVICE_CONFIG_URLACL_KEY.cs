using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_URLACL_KEY
    {
        public HTTP_SERVICE_CONFIG_URLACL_KEY(string pUrlPrefix)
        {
            this.pUrlPrefix = pUrlPrefix;
        }

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pUrlPrefix;
    }
}