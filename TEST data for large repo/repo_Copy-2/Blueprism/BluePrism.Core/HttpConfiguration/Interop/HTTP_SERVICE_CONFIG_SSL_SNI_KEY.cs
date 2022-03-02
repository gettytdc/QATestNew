using System.Runtime.InteropServices;

namespace BluePrism.Core.HttpConfiguration.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_SNI_KEY
    {
        //public SOCKADDR_STORAGE IpPort;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Host;
    }
}