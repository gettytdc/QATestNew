using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Wraps a .NET TcpClient object so we can use the ITcpClient interface in place of TcpClient.
    /// </summary>
    internal class TcpClientWrapper : ITcpClient
    {
        private readonly TcpClient _tcpClient;

        public TcpClientWrapper(TcpClient client)
        {
            _tcpClient = client;
        }

        /// <inheritdoc />
        public bool Connected => _tcpClient.Connected;

        /// <inheritdoc />
        public bool NoDelay { get => _tcpClient.NoDelay; set => _tcpClient.NoDelay = value; }

        /// <inheritdoc />
        public int SendTimeout { get => _tcpClient.SendTimeout; set => _tcpClient.SendTimeout = value; }

        /// <inheritdoc />
        public int ReceiveTimeout { get => _tcpClient.ReceiveTimeout; set => _tcpClient.ReceiveTimeout = value; }

        /// <inheritdoc />
        public void Close() => _tcpClient.Close();

        /// <inheritdoc />
        public bool Connect(IPAddress address, int port, TimeSpan timeout) => _tcpClient != null && _tcpClient.ConnectAsync(address, port).Wait(timeout);

        /// <inheritdoc />
        public Stream GetStream() => _tcpClient.GetStream();
        public int ReceiveBufferSize { get => _tcpClient.ReceiveBufferSize; set => _tcpClient.ReceiveBufferSize = value; }
        public int SendBufferSize { get => _tcpClient.SendBufferSize; set => _tcpClient.SendBufferSize = value; }

    }
}
