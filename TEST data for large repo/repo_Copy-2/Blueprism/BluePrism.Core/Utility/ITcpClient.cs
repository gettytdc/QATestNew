using System;
using System.IO;
using System.Net;

namespace BluePrism.Core.Utility
{

    public interface ITcpClient
    {
        /// <summary>
        /// Returns the network stream used to send and receive data.
        /// </summary>
        /// <returns></returns>
        Stream GetStream();

        /// <summary>
        /// Gets a value indicating whether the underlying Socket for a TCPClient is connected to a remote host.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets or sets a value that disables a delay when send or receive buffers are not full.
        /// </summary>
        bool NoDelay { get; set; }

        /// <summary>
        /// Gets or sets the amount of time a TCPClient will wait for a send operation to complete successfully.
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of time a TCPClient will wait to receive data once a read operation is initiated.
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Connects the client to the remote host using the specified IP address and port. 
        /// </summary>
        /// <param name="address">Address of the remote host</param>
        /// <param name="port">Port of the remote host.</param>
        /// <param name="timeout">Time to wait for a successful connections before timing out.</param>
        /// <returns>true if a successful connection is made. False is a connection cannot be made before the timeout is reached.</returns>
        bool Connect(IPAddress address, int port, TimeSpan timeout);

        /// <summary>
        /// Disposes this TCPClient instance and requests that the TCP connection be closed.
        /// </summary>
        void Close();
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
    }
}
