using System;

namespace BluePrism.BPServer.UrlReservations
{
    /// <summary>
    /// Determines type of match between URLs used to create URL reservations.
    /// </summary>
    public class UrlReservationMatcher
    {
        /// <summary>
        /// Compares 2 reservation URLs and determines whether they match or conflict. Conflicting
        /// URLs cannot be used to create separate URL reservations. Conflicting URLs are http and
        /// https URLs using any of the following:
        /// <list type="bullet">
        /// <item><description>The same address on each protocol</description></item>
        /// <item><description>Different addresses on each protocol (unless both are IP addresses)</description></item>
        /// <item><description>A wildcard address on one protocol and non-IP address on the other</description></item>
        /// <item><description>A wildcard address and a specific URL on the same protocol</description></item>
        /// </list>
        /// </summary>
        /// <param name="reservationUrl1">The first reservation URL to compare. This should be a 
        /// valid URL or the strong wildcard used by URL reservations using the + character</param>
        /// <param name="reservationUrl2">The second reservation URL to compare. This should be a 
        /// valid URL or the strong wildcard used by URL reservations using the + character</param>
        public static UrlReservationMatchType Compare(string reservationUrl1, string reservationUrl2)
        {
            var url1 = BindingProperties.ParseReservationUrl(reservationUrl1);
            var url2 = BindingProperties.ParseReservationUrl(reservationUrl2);

            /// Create a decision table that will be used to calculate the match type
            UrlReservationMatchingDecisionTable decisions = new UrlReservationMatchingDecisionTable();              
                                
            decisions.Add(UrlReservationMatchType.None, 
                    Not(SamePort)
                );

            decisions.Add(UrlReservationMatchType.ExactMatch, 
                
                    SamePort, 
                    ExactURLMatch 
                );

            decisions.Add(UrlReservationMatchType.Conflict, 
                    SameProtocol,
                    SamePort,
                    ExactlyOneIsWildcardAddress 
                    );

            decisions.Add(UrlReservationMatchType.None, 
                    SamePort, 
                    Not(ExactURLMatch), 
                    SameProtocol, 
                    NeitherIsWildcardAddress
                );

            decisions.Add(UrlReservationMatchType.Conflict, 
                    SamePort, 
                    Not(ExactURLMatch), 
                    Not(SameProtocol), 
                    BindingAddressMatch 
                );

            decisions.Add(UrlReservationMatchType.Conflict, 
                    SamePort, 
                    Not(ExactURLMatch), 
                    Not(SameProtocol), 
                    Not(BindingAddressMatch), 
                    NeitherIsIpAddress ,
                    Not(ExactlyOneIsWildcardAddress)
                );

            decisions.Add(UrlReservationMatchType.None,  
                    SamePort, 
                    Not(ExactURLMatch), 
                    Not(SameProtocol), 
                    Not(BindingAddressMatch), 
                    Not(NeitherIsIpAddress),
                    Not(ExactlyOneIsWildcardAddress)
                );

            return decisions.MakeDecision(url1, url2);

        }
        
        /// <summary>
        /// Returns true if both urls have the same port
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            SamePort = (url1, url2) => url1.Port == url2.Port ;
        
        /// <summary>
        /// Returns true if both urls exactly match
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            ExactURLMatch = (url1, url2) => url1.BindingReservationUrl.Equals(url2.BindingReservationUrl, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns true if both urls use the same protocol
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            SameProtocol = (url1, url2) => url1.Secure == url2.Secure;

        /// <summary>
        /// Returns true if one url is a wildcard address and the other is not
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool>
            ExactlyOneIsWildcardAddress = (url1, url2) => url1.IsWildcardAddress ^ url2.IsWildcardAddress;

        /// <summary>
        /// Returns true if neither url is a wildcard address
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            NeitherIsWildcardAddress = (url1, url2) => !url1.IsWildcardAddress && !url2.IsWildcardAddress;

        /// <summary>
        /// Returns true if the binding addresses match
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            BindingAddressMatch = (url1, url2) => url1.Address.Equals(url2.Address, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns true if neither url uses an ip address
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            NeitherIsIpAddress = (url1, url2) => !url1.IsIpAddress && !url2.IsIpAddress;
    
        /// <summary>
        /// Returns a Func that represents the negation of the inputted Func
        /// </summary>
        private static Func<BindingProperties, BindingProperties, bool> 
            Not(Func<BindingProperties, BindingProperties, bool> f)
        {
            return (x, y) => !f(x,y); 
        } 

    }
}