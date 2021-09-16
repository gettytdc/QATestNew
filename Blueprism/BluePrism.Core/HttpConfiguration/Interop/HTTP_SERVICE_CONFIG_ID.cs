// ReSharper disable InconsistentNaming
namespace BluePrism.Core.HttpConfiguration.Interop
{
    /// <summary>
    /// Type used internally for unmanaged calls to httpapi.dll functions
    /// </summary>
    enum HTTP_SERVICE_CONFIG_ID
    {
        HttpServiceConfigIPListenList = 0,
        HttpServiceConfigSSLCertInfo,
        HttpServiceConfigUrlAclInfo,
        HttpServiceConfigTimeout,
        HttpServiceConfigCache,
        HttpServiceConfigSslSniCertInfo,
        HttpServiceConfigMax
    }
}