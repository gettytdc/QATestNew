
using System.Net.Sockets;


namespace BluePrism.Core.Utility
{

    public class TcpClientFactory : ITcpClientFactory
    {
        public ITcpClient CreateTcpClient(AddressFamily addressFamily)
        {
            return new TcpClientWrapper(new TcpClient(addressFamily));
        }
    }
}
