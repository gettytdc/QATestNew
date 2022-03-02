
using System.Net;
using System.Net.NetworkInformation;


namespace BluePrism.Core.Network
{
    /// <summary>
    /// Interface for querying the network interfaces on the PC.
    /// </summary>
    public interface INetworkInterfaces
    {
        /// <summary>
        /// Returns true if there are any active network intefaces on the PC which support the specified IP version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        bool AnyNetworkInterfaceSupportsIPVersion(NetworkInterfaceComponent version);

        /// <summary>
        /// Returns the IPv6 loopback address if IPv6 is enabled, otherwise the IPv4 loopback address.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        IPAddress GetLoopbackAddress();
    }
}
