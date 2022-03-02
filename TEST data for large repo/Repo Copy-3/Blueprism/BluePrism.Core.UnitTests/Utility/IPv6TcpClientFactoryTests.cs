using BluePrism.BPCoreLib;
using BluePrism.Core.Network;
using BluePrism.Core.Utility;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace BluePrism.Core.UnitTests.Utility
{
    [TestFixture]
    public class IPv6TcpClientFactoryTests
    {

        private string HostName = "host1";
        private int Port = 1234;
        private IPAddress IPv4Address1 = IPAddress.Parse("192.168.0.1");
        private IPAddress IPv4Address2 = IPAddress.Parse("192.168.0.2");
        private IPAddress IPv6Address1 = IPAddress.Parse("2001:db8::1");
        private IPAddress IPv6Address2 = IPAddress.Parse("2001:db8::2");

        private string LocalHostname = "client1";

        // Keeps track of what connection attempts the Mock TCP clients make during a test.
        private List<IPAddress> connectionAttempts = new List<IPAddress>();

        // IP addresses which mock TCP client instances will simulate successful connections to during a test.
        private List<IPAddress> allowSuccessfulConnectionsFrom = new List<IPAddress>();

        // IP addresses in which mock TCP client instances will throw an exception when connecting to.
        private List<IPAddress> throwExceptionWhenConnectingTo = new List<IPAddress>();

        [SetUp]
        public void Setup()
        {
            connectionAttempts.Clear();
            allowSuccessfulConnectionsFrom.Clear();
            throwExceptionWhenConnectingTo.Clear();
        }


        /// <summary>
        /// When the hostname to connect to resolves to 0 IP addresses, an exception is expected.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToNoAddresses_ExceptionThrown()
        {

            // setup.

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns empty list when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { });


            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);


            var tcpClientMock = new Moq.Mock<ITcpClient>();
            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(tcpClientMock.Object);

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);


            // test
            var exception = Assert.Throws<BluePrismException>(() => classUnderTest.CreateTcpClient(HostName, 1234, LocalHostname));


            // assert 

            // Expected exception when DNS returns no addresses for host.
            Assert.AreEqual("Unable to connect to resource. Unable to get IP address for host.", exception.Message);

        }

        /// <summary>
        /// When the hostname to connect to resolves to a single IPv4 address and the connection to the host can be made, the method should return a TcpClient instance in the connected state.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv4AddressOnly_ClientConnected()
        {

            // setup

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv4Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);


            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);


            // test
            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            // assert


            Assert.AreEqual(1, connectionAttempts.Count); // there should only have been a single connection attempt.
            // ensure the TcpClient instance returned from the method is the one we would expect. Should be in the connected state, and have properties matching the IPAddress we simulated a successful connection on.
            Assert.AreEqual(true, tcpClient.Connected); 
            Assert.AreEqual(IPv4Address1, tcpClient.ConnectedRemoteEndpointAddress); 
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves to a single IPv4 address, but the connection to the host can't be made, then the method should throw an exception.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv4AddressOnly_ClientNotConnected()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();


            // DNS returns an IPv4 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv4Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            var exception = Assert.Throws<BluePrismException>(() => classUnderTest.CreateTcpClient(HostName, Port, LocalHostname));

            Assert.Contains(IPv4Address1, connectionAttempts);
            Assert.AreEqual("Connection to the resource timed out.", exception.Message);
        }


        /// <summary>
        /// When the hostname to connect to resolves to a single IPv6 address and the connection to the host can be made, the method should return a TcpClient instance in the connected state.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AddressOnly_ClientConnected()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv6Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv6Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;


            Assert.AreEqual(1, connectionAttempts.Count);
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv6Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves to a single IPv6 address, but the connection to the host can't be made, then the method should throw an exception.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AddressOnly_ClientNotConnected()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv6Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);


            var exception = Assert.Throws<BluePrismException>(() => classUnderTest.CreateTcpClient(HostName, Port, LocalHostname));

            Assert.Contains(IPv6Address1, connectionAttempts);
            Assert.AreEqual("Connection to the resource timed out.", exception.Message);
        }


        /// <summary>
        /// When the hostname to connect to resolves a IPv6 and IPv4 address, the connection should be first attempted using the IPv6 address. If this connection can be make the method should return a TcpClient instance in the connected state.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AndIPv4Address_ClientConnectedOnIPv6Address()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv4Address1, IPv6Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv6Address1);
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;


            Assert.AreEqual(1, connectionAttempts.Count);
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv6Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves a IPv6 and IPv4 address, the connection should be first attempted using the IPv6 address. 
        /// If this connection cannot be made the method should fallback to the IPv4 address. If this is successful the method should return a TcpClient instance in the connected state.
        /// This test simulates a timeout when connecting to using the IPv6 address.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AndIPv4Address_ClientConnectionRefusedOnIPv6Address_FallbackToIPv4()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv6Address1, IPv4Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            Assert.AreEqual(2, connectionAttempts.Count);
            Assert.Contains(IPv6Address1, connectionAttempts);
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv4Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves a IPv6 and IPv4 address, the connection should be first attempted using the IPv6 address. 
        /// If this connection cannot be made the method should fallback to the IPv4 address. If this is successful the method should return a TcpClient instance in the connected state.
        /// This test simulates an exception thrown from the TcpClient.Connect method when connecting to using the IPv6 address.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AndIPv4Address_ClientConnectionExceptionOnIPv6Address_FallbackToIPv4()
        {
            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv6Address1, IPv4Address1 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);

            // simulate the connect method throwing an exception when trying to connect using the IPv6 address.
            throwExceptionWhenConnectingTo.Add(IPv6Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            // There should have been an attempt to connect to the IPv6 address before the IPv4 one.
            Assert.Contains(IPv6Address1, connectionAttempts);

            // But the successful connection should be on the IPv4 address.
            // ensure the TcpClient instance returned from the method is the one we would expect. Should be in the connected state, and have properties matching the IPAddress we simulated a successful connection on.
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv4Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves to two IPv6 addresses, the method should attempt the next IPv6 address if the connection attempt on the first one fails.
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToTwoIPv6Addresses_ClientConnectionRefusedOnFirstAddress_ConnectToSecondIPv6Address()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns 2 IPv6 addresses when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv6Address1, IPv6Address2 });

            // network interfaces on client support IPv4 and IPv6.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv6Address2);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            // ensure the TcpClient instance returned from the method is the one we would expect. Should be in the connected state, and have properties matching the IPAddress we simulated a successful connection on.
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv6Address2, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to resolves a IPv6 and IPv4 address, if the client network interfaces support only IPv4 then the connection should be made using the IPv4 address.
        /// The IPv6 address should not be used.
        /// If this is successful the method should return a TcpClient instance in the connected state. 
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AndIPv4Address_ClientSupportsOnlyIPv4_ConnectToIPv4Address()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            // all loopback addresses should be ignored
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(
                new IPAddress[] { IPAddress.IPv6Loopback,
                                  IPv6Address1,
                                  IPAddress.Loopback,
                                  IPv4Address1 });

            // network interfaces on client support IPv4 only.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(false);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            // it will connect successfully to both the IPv6 and IPv4 addresses, but the IPv6 one shouldn't be attempted as the network interface on the client does not support IPv6.
            allowSuccessfulConnectionsFrom.Add(IPv6Address1);
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            // The IPv6 connection should not have been attempted since the network interface doesn't support it.
            CollectionAssert.DoesNotContain(connectionAttempts, IPv6Address1);

            // ensure the TcpClient instance returned from the method is the one we would expect. Should be in the connected state, and have properties matching the IPAddress we simulated a successful connection on.
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv4Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }


        /// <summary>
        /// When the hostname to connect to resolves a IPv6 and IPv4 address, if the client network interfaces support only IPv6 then the connection should be made using the IPv6 address.
        /// The IPv4 address should not be used.
        /// If this is successful the method should return a TcpClient instance in the connected state. 
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToIPv6AndIPv4Address_ClientSupportsOnlyIPv6_ConnectToIPv6Address()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            // we add the ipv6 loopback address. This should be ignored
            dnsServiceMock.Setup(x => x.GetHostAddresses(HostName)).Returns(new IPAddress[] { IPv4Address1, IPAddress.IPv6Loopback, IPv6Address1 });

            // network interfaces on client support IPv6 only.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(false);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            // mock tcp client will simulate successful connection when asked to connect on this address.
            allowSuccessfulConnectionsFrom.Add(IPv6Address1);
            allowSuccessfulConnectionsFrom.Add(IPv4Address1);

            var tcpClient = classUnderTest.CreateTcpClient(HostName, Port, LocalHostname) as MockTcpClient;

            // The IPv4 connection should not have been attempted since the network interface doesn't support it.
            CollectionAssert.DoesNotContain(connectionAttempts, IPv4Address1);

            // ensure the TcpClient instance returned from the method is the one we would expect. Should be in the connected state, and have properties matching the IPAddress we simulated a successful connection on.
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPv6Address1, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }


        /// <summary>
        /// When the hostname to connect to is the name of the local host, and the client network interface supports IPv6 then the connection should be made using the IPv6Loopback address.
        /// If this is successful the method should return a TcpClient instance in the connected state. 
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToLocalhost_ClientSupportsIPv6_ConnectToIPv6LoopbackAddress()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(LocalHostname)).Returns(new IPAddress[] { IPv6Address1, IPv4Address1 });

            // network interfaces on client support IPv6 only.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(false);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(true);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.IPv6Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            allowSuccessfulConnectionsFrom.Add(IPAddress.IPv6Loopback);

            // Connecting to the local machine. The IP address used should be the IPv6 Loopback address.
            var tcpClient = classUnderTest.CreateTcpClient(LocalHostname, Port, LocalHostname) as MockTcpClient;

            // There should be a single connection attempt to IPAddress.IPv6Loopback.
            Assert.AreEqual(1, connectionAttempts.Count);
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPAddress.IPv6Loopback, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

        /// <summary>
        /// When the hostname to connect to is the name of the local host, and the client network interface supports IPv4 then the connection should be made using the IPv4 Loopback address.
        /// If this is successful the method should return a TcpClient instance in the connected state. 
        /// </summary>
        [Test]
        public void CreateTcpClient_HostnameResolvesToLocalhost_ClientSupportsIPv4_ConnectToIPv4LoopbackAddress()
        {

            var dnsServiceMock = new Moq.Mock<IDNSService>();

            // DNS returns an IPv4 and IPv6 address when resolving hostname.
            dnsServiceMock.Setup(x => x.GetHostAddresses(LocalHostname)).Returns(new IPAddress[] { IPv4Address1, IPv6Address1 });

            // network interfaces on client support IPv4 only.
            var networkInterfacesMock = new Moq.Mock<INetworkInterfaces>();
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv4)).Returns(true);
            networkInterfacesMock.Setup(x => x.AnyNetworkInterfaceSupportsIPVersion(System.Net.NetworkInformation.NetworkInterfaceComponent.IPv6)).Returns(false);
            networkInterfacesMock.Setup(x => x.GetLoopbackAddress()).Returns(IPAddress.Loopback);

            var tcpClientFactoryMock = new Moq.Mock<ITcpClientFactory>();
            tcpClientFactoryMock.Setup(x => x.CreateTcpClient(Moq.It.IsAny<AddressFamily>())).Returns(new MockTcpClient(connectionAttempts, allowSuccessfulConnectionsFrom, throwExceptionWhenConnectingTo));

            IPv6TcpClientFactory classUnderTest = new IPv6TcpClientFactory(networkInterfacesMock.Object, dnsServiceMock.Object, tcpClientFactoryMock.Object);

            allowSuccessfulConnectionsFrom.Add(IPAddress.Loopback);

            // Connecting to the local machine. The IP address used should be the IPv4 Loopback address.
            var tcpClient = classUnderTest.CreateTcpClient(LocalHostname, Port, LocalHostname) as MockTcpClient;

            // There should be a single connection attempt to IPAddress.Loopback.
            Assert.AreEqual(1, connectionAttempts.Count);
            Assert.AreEqual(true, tcpClient.Connected);
            Assert.AreEqual(IPAddress.Loopback, tcpClient.ConnectedRemoteEndpointAddress);
            Assert.AreEqual(Port, tcpClient.ConnectedRemoteEndpointPort);
        }

    }


    internal class MockTcpClient : ITcpClient
    {

        public IPAddress ConnectedRemoteEndpointAddress { get; private set; }
        public int ConnectedRemoteEndpointPort { get; private set; }

        private bool _connected = false;

        private List<IPAddress> _connectionAttempts;
        private List<IPAddress> _allowedConnections;
        private List<IPAddress> _throwExceptionConnectingTo;

        /// <summary>
        /// Creates a mock tcp client. 
        /// </summary>
        /// <param name="addressToConnect">The IP address expected by the Connect method for a successful connection. All other IP addresses passed to the connect method will result in a failed connection.</param>
        public MockTcpClient(List<IPAddress> connectionAttempts, List<IPAddress> allowedConnections, List<IPAddress> throwExceptionConnectingTo)
        {
            _connectionAttempts = connectionAttempts;
            _allowedConnections = allowedConnections;
            _throwExceptionConnectingTo = throwExceptionConnectingTo;
        }

        public bool Connected => _connected;

        public bool NoDelay { get; set; }
        public int SendTimeout { get ; set ; }
        public int ReceiveTimeout { get; set; }

        public void Close()
        {
            
        }

        /// <summary>
        /// This method simulates either connecting or failing to connect to a remote endpoint using the lists of IP addresses passing into the constructor.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Connect(IPAddress address, int port, TimeSpan timeout)
        {

            _connectionAttempts.Add(address);

            if (_throwExceptionConnectingTo.Contains(address))
            {
                throw new InvalidOperationException("error");
            }

            _connected = _allowedConnections.Contains(address);

            if(_connected)
            { 
                ConnectedRemoteEndpointAddress = address;
                ConnectedRemoteEndpointPort = port;
            }

            return _connected;
        }

        public Stream GetStream()
        {
            throw new NotImplementedException();
        }

        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }

    }

}
