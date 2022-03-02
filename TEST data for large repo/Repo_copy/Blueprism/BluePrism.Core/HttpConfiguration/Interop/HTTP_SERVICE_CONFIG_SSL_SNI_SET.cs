using System.Runtime.InteropServices;

namespace BluePrism.Core.HttpConfiguration.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_SNI_SET
    {
        public HTTP_SERVICE_CONFIG_SSL_SNI_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_SSL_PARAM ParamDesc;
    }
}