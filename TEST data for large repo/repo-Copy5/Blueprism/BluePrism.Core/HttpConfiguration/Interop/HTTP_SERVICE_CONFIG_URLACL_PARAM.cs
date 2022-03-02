using System.Runtime.InteropServices;

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct HTTP_SERVICE_CONFIG_URLACL_PARAM
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pStringSecurityDescriptor;
    }
}