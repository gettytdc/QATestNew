using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_KEY
    {
        public IntPtr pIpPort;

        public HTTP_SERVICE_CONFIG_SSL_KEY(IntPtr pIpPort)
        {
            this.pIpPort = pIpPort;
        }
    }
}