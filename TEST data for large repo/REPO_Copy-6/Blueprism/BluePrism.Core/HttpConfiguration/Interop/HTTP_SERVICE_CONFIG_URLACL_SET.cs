using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace BluePrism.Core.HttpConfiguration.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_URLACL_SET
    {
        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;
        public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;
    }
}