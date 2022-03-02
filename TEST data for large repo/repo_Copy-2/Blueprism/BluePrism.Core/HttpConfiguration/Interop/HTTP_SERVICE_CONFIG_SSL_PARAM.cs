using System;
using System.Runtime.InteropServices;

namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct HTTP_SERVICE_CONFIG_SSL_PARAM
    {
        public int SslHashLength;
        public IntPtr pSslHash;
        public Guid AppId;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pSslCertStoreName;
        public uint DefaultCertCheckMode;
        public int DefaultRevocationFreshnessTime;
        public int DefaultRevocationUrlRetrievalTimeout;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlIdentifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pDefaultSslCtlStoreName;
        public uint DefaultFlags;
    }
}