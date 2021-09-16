using System.Runtime.InteropServices;

namespace BluePrism.Core.HttpConfiguration.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_SNI_QUERY
    {
        public HTTP_SERVICE_CONFIG_QUERY_TYPE QueryDesc;
        public HTTP_SERVICE_CONFIG_SSL_SNI_KEY KeyDesc;
        public uint dwToken;
    }
}