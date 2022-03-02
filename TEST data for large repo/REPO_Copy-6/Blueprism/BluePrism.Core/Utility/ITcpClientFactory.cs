using System.Net.Sockets;

namespace BluePrism.Core.Utility
{
    public interface ITcpClientFactory
    {
        ITcpClient CreateTcpClient(AddressFamily addressFamily);
    }
}
