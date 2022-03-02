using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_QUERY
    {
        public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
        public HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;
        public uint dwToken;
    }
}