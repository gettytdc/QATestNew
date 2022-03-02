namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Creates TCPClient instances which connect to the host using either IPv6 or IPv4 depending on the IP addresses resolved in DNS, and whether IPv6 / IPv4 are enabled on the client's network interfaces.
    /// TcpClient instances returned from this method are already in the connected state.
    /// Throws Blue Prism exception in the event a connection cannot be made.
    /// </summary>
    public interface IIPv6TcpClientFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname">Hostname of the host to connect to</param>
        /// <param name="port">port to connect to</param>
        /// <param name="localHostname">Hostname of this local machine</param>
        /// <returns></returns>
        ITcpClient CreateTcpClient(string hostname, int port, string localHostname);
    }
}
