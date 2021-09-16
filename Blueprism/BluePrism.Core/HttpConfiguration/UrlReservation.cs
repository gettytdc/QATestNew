using System;

namespace BluePrism.Core.HttpConfiguration
{
    /// <summary>
    /// Contains details of a URL Reservation within a server's HTTP configuration,
    /// which applies permissions relating to HTTP requests on a given URL
    /// </summary>
    public class UrlReservation
    {
        /// <summary>
        /// Creates a new UrlReservation
        /// </summary>
        /// <param name="url">The URL that has been reserved</param>
        /// <param name="securityDescriptor">A SecurityDescriptor containing
        /// the permissions on the URL</param>
        public UrlReservation(string url, SecurityDescriptor securityDescriptor)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            Url = url;
            SecurityDescriptor = securityDescriptor;
        }

        /// <summary>
        /// Identifies the URL that has been reserved
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Describes permissions assigned to the URL
        /// </summary>
        public SecurityDescriptor SecurityDescriptor { get; private set; }
    }
}