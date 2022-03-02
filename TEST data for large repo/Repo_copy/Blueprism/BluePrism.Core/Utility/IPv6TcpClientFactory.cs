using BluePrism.BPCoreLib;
using BluePrism.Core.Network;
using BluePrism.Server.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BluePrism.Core.Utility
{
    public class IPv6TcpClientFactory : IIPv6TcpClientFactory
    {
        INetworkInterfaces _networkInterfaces;
        IDNSService _dnsService;
        ITcpClientFactory _tcpClientFactory;

        const int ConnectTimeout = 5000;
        const int SendTimeout = 5000;
        const int ReceiveTimeout = 5000;

        public IPv6TcpClientFactory(INetworkInterfaces networkInterfaces, IDNSService dnsService, ITcpClientFactory tcpClientFactory)
        {
            _networkInterfaces = networkInterfaces ?? throw new ArgumentNullException(nameof(networkInterfaces));
            _dnsService = dnsService ?? throw new ArgumentNullException(nameof(dnsService));
            _tcpClientFactory = tcpClientFactory ?? throw new ArgumentNullException(nameof(tcpClientFactory));
        }

        /// <summary>
        /// Creates TCPClient instances which connect to the host using either IPv6 or IPv4 depending on the IP addresses resolved in DNS, and whether IPv6 / IPv4 are enabled on the client's network interfaces.
        /// TcpClient instances returned from this method are already in the connected state.
        /// Throws Blue Prism exception in the event a connection cannot be made.
        /// </summary>
        public ITcpClient CreateTcpClient(string hostname, int port, string localResourceName)
        {
            // if connecting to the local machine use the appropriate loopback address (IPv6 / IPv4).
            if(localResourceName == hostname)
            {
                return DoConnect(_networkInterfaces.GetLoopbackAddress(), port);
            }

            // resolve hostname to a list of IP addresses.
            var hostAddresses = _dnsService.GetHostAddresses(hostname);

            // if none, then throw.
            if (!hostAddresses.Any())
            {
                throw new BluePrismException(Properties.Resource.UnableToConnect_NoIPAddress);
            }

            Exception lastException = null;

            List<IPAddress> orderedAddresses = new List<IPAddress>();

            // try to connect using any IPv6 addresses first if our network interface supports IPv6.
            if (_networkInterfaces.AnyNetworkInterfaceSupportsIPVersion(NetworkInterfaceComponent.IPv6))
            {
                orderedAddresses.AddRange(
                    hostAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6
                                             && !x.Equals(IPAddress.IPv6Loopback)));
            }

            // then add any IPv4 addresses after the IPv6 ones.
            if (_networkInterfaces.AnyNetworkInterfaceSupportsIPVersion(NetworkInterfaceComponent.IPv4))
            {
                orderedAddresses.AddRange(
                    hostAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork
                                             && !x.Equals(IPAddress.Loopback)));
            }

            foreach(var address in orderedAddresses)
            {
                try
                {
                    return DoConnect(address, port);
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            
            // all connection attempts have failed. If there was an exception thrown at any point, rethrow the last one.
            // otherwise throw a more generic exception.
            throw lastException ?? throw new BluePrismException(Properties.Resource.UnableToConnect_IPVersionMismatch);
        }


        private ITcpClient DoConnect(IPAddress address, int port)
        {
            var client = _tcpClientFactory.CreateTcpClient(address.AddressFamily);

            try
            {
                client.NoDelay = true;
                client.SendTimeout = SendTimeout;
                client.ReceiveTimeout = ReceiveTimeout;

                if (!client.Connect(address, port, TimeSpan.FromMilliseconds(ConnectTimeout)))
                {
                    throw new BluePrismException(Properties.Resource.UnableToConnect_ConnectionTimeout);
                }

                return client;

            }
            catch (Exception)
            {
                client.Close();
                throw;
            }
        }

    }
}
