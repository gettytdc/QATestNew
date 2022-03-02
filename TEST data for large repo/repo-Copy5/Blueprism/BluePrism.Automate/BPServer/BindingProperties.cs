using BluePrism.Core.Utility;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace BluePrism.BPServer
{
    /// <summary>
    /// Contains information about the binding properties of a server configuration
    /// and what URL reservation that would apply to requests made with these properties.
    /// </summary>
    public class BindingProperties
    {
        /// <summary>
        /// Parses a reservation URL and initialises a new instance of BindingProperties
        /// based on the protocol, address and port used
        /// </summary>
        /// <param name="input">A string containing a URL for a URL reservation</param>
        /// <returns></returns>
        public static BindingProperties ParseReservationUrl(string input)
        {
            BindingProperties bindingProperties;
            if (!TryParseReservationUrl(input, out bindingProperties))
            {
                throw new FormatException();
            }
            return bindingProperties;
        }

        /// <summary>
        /// Parses a reservation URL and initialises a new instance of BindingProperties
        /// based on the protocol, address and port used
        /// </summary>
        /// <param name="input">A string containing a URL for a URL reservation</param>
        /// <param name="bindingProperties">Variable to be initialised with the BindingProperties
        /// if successfully parsed</param>
        /// <returns>Indicates whether the input was successfully parsed</returns>
        public static bool TryParseReservationUrl(string input, out BindingProperties bindingProperties)
        {
            var match = Regex.Match(input, @"^(?<protocol>https?)://(?<address>\+|\*|[a-zA-Z0-9._:[\]\-%]+):(?<port>\d+)/(?:.+/)?$");

            if (!match.Success)
            {
                bindingProperties = null;
            }
            else
            {
                string address = match.Groups["address"].Value;
                address = IPAddressHelper.UnescapeIPv6Address(address);
                if (address == "+" || address == "*") address = "";
                int port = int.Parse(match.Groups["port"].Value);
                bool secure = match.Groups["protocol"].Value == "https";
                bindingProperties = new BindingProperties(address, port, secure);
            }
            return bindingProperties != null;
        }

        /// <summary>
        /// Creates a new instance of BindingProperties
        /// </summary>
        /// <param name="address">The address (host name, IP address or an empty
        /// string to listen on any address</param>
        /// <param name="port">The port</param>
        /// <param name="secure">Whether to listen on a secure connection</param>
        public BindingProperties(string address, int port, bool secure)
        {
            Address = address;
            Port = port;
            Secure = secure;
            IPAddress ip;
            IsIpAddress = IPAddress.TryParse(address, out ip);



            BindingReservationUrl = string.IsNullOrEmpty(address)
                ? WildcardReservationUrl
                : string.Format("{0}://{1}:{2}/bpserver/", Protocol, IPAddressHelper.EscapeForURL(address), port);
        }

        /// <summary>
        /// The server configuration's binding address. This can be host name or an 
        /// IP address.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Indicates whether the address is a valid IP address
        /// </summary>
        public bool IsIpAddress { get; private set; }

        /// <summary>
        /// The transport layer protocol used by the server configration 
        /// i.e. http or https.
        /// </summary>
        public string Protocol { get { return Secure ? "https" : "http"; } }

        /// <summary>
        /// The port specified in the server configuration
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Whether the configuration is using a secure transport mode
        /// </summary>
        public bool Secure { get; private set; }

        /// <summary>
        /// Indicates whether the binding applies to any address on the 
        /// selected port
        /// </summary>
        public bool IsWildcardAddress
        {
            get { return string.IsNullOrEmpty(Address); }
        }

        /// <summary>
        /// The URL of a URL reservation that would apply to requests made using the 
        /// Binding Property's protocol, port and address - will be the same as 
        /// WildcardReservationUrl if the address is empty.
        /// </summary>
        public string BindingReservationUrl { get; private set; }

        /// <summary>
        /// The URL of a URL reservation that would apply to requests made with the 
        /// Binding Property's protocol and port using any IP address or hostname
        /// </summary>
        public string WildcardReservationUrl
        {
            get
            {
                return string.Format("{0}://+:{1}/bpserver/", Protocol, Port);
            }
        }

        /// <summary>
        /// Checks whether a reservation URL would apply to requests made using the 
        /// Binding Property's protocol, port and address
        /// </summary>
        /// <param name="reservationUrl">The URL to test against the binding properties</param>
        /// <returns>True if the reservationUrl would apply to requests made using this 
        /// BindingProperty's protocol, port and address</returns>
        public bool MatchesReservationUrl(string reservationUrl)
        {
            BindingProperties props = ParseReservationUrl(reservationUrl);

            if (
                props.Secure == this.Secure && 
                props.Address.Equals(this.Address, StringComparison.OrdinalIgnoreCase) && 
                props.Port == this.Port
                )
            {
                return true;
            }
            
            return false;
        }
    }
}
