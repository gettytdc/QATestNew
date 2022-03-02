
using System.Net;

namespace BluePrism.Core.Network
{
    // Interface for querying DNS
    public interface IDNSService
    {
        /// <summary>
        /// Resolves the hostname to a list of IP addresses.
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        IPAddress[] GetHostAddresses(string hostname);

        /// <summary>
        /// Returns the hostname of this computer.
        /// </summary>
        /// <returns></returns>
        string GetHostName();
    }
}
