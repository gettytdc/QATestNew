using System;
using System.Net;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Helper functionality for working with IP addresses
    /// </summary>
    // ReSharper disable once InconsistentNaming (staying consistent with naming of
    // System.Net.IPAddress)
    public static class IPAddressHelper
    {
        /// <summary>
        /// Indicates whether a value is a valid IP address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsValid(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            IPAddress ipAddress;
            return IPAddress.TryParse(address, out ipAddress);
        }

        /// <summary>
        /// Escapes IPv6 addresses by enclosing them in square brackets so they can be used in a url. Anything which isn't an IPv6 address is unaffected.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string EscapeForURL(string address)
        {
            if (IPAddressHelper.IsIPv6(address))
            {
                return $"[{address}]";
            }

            return address;
        }

        /// <summary>
        /// Removes the square brackets from an escaped IPv6 address.
        /// Returns the address untouched if it is not enclosed in square brackets.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string UnescapeIPv6Address(string address)
        {
            return address.Trim('[', ']');
        }

        /// <summary>
        /// Returns true if the address is an IPv4 address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsIPv4(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            IPAddress ipAddress;
            if(IPAddress.TryParse(address, out ipAddress))
            {
                return ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the address is an IPv6 address. The IPv6 address must not be enclosed in square brackets.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsIPv6(string address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            IPAddress ipAddress;
            if (IPAddress.TryParse(address, out ipAddress))
            {
                return ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
            }

            return false;
        }
    }
}