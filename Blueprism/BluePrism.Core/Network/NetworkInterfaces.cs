using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;

namespace BluePrism.Core.Network
{
    public class NetworkInterfaces : INetworkInterfaces
    {
        /// <inheritdoc />
        public bool AnyNetworkInterfaceSupportsIPVersion(NetworkInterfaceComponent version)
        {
            switch (version)
            {
                case NetworkInterfaceComponent.IPv6:
                    return IsIpv6Supported();
                case NetworkInterfaceComponent.IPv4:
                    return Socket.OSSupportsIPv4;
                default:
                    throw new InvalidEnumArgumentException(nameof(version), (int)version, typeof(NetworkInterfaceComponent));
            }
        }

        /// <inheritdoc />
        public IPAddress GetLoopbackAddress()
        {
            return AnyNetworkInterfaceSupportsIPVersion(NetworkInterfaceComponent.IPv6) ?
                 IPAddress.IPv6Loopback : IPAddress.Loopback;
        }

        private bool IsIpv6Supported()
        {
            // get all ip addresses supported by the network interfaces on this host.
            // if any of them use ipv6 addressing (excluding the loopback address)
            // then the interface is using an ipv6 adapter
            return Dns.GetHostAddresses(Dns.GetHostName()).ToList().
                Any(ip => ip.AddressFamily == AddressFamily.InterNetworkV6
                          && !ip.Equals(IPAddress.IPv6Loopback));
        }
    }
}
