
using System.Net;


namespace BluePrism.Core.Network
{
    public class DNSService : IDNSService
    {
        /// <inheritdoc />
        public IPAddress[] GetHostAddresses(string hostname) => Dns.GetHostAddresses(hostname);

        /// <inheritdoc />
        public string GetHostName() => Dns.GetHostName();

    }
}
