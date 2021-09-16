using System.Collections.Generic;
using System.Linq;
using BluePrism.Core.HttpConfiguration.Interop;

namespace BluePrism.Core.HttpConfiguration
{
    /// <summary>
    /// Provides management of the local machine's HTTP configuration.
    /// </summary>
    public class HttpConfigurationService
    {
        /// <summary>
        /// Adds a URL reservation
        /// </summary>
        /// <param name="reservation">The details of the reservation to add</param>
        public void AddUrlReservation(UrlReservation reservation)
        {
            using (var wrapper = new HttpApiWrapper())
            {
                var inputConfig = new HTTP_SERVICE_CONFIG_URLACL_SET
                {
                    KeyDesc = new HTTP_SERVICE_CONFIG_URLACL_KEY(reservation.Url),
                    ParamDesc = new HTTP_SERVICE_CONFIG_URLACL_PARAM
                    {
                        pStringSecurityDescriptor = reservation.SecurityDescriptor.SsdlString
                    }
                };
                wrapper.Set(HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                    inputConfig);
            }
        }

        /// <summary>
        /// Deletes the URL reservation with the specified URL
        /// </summary>
        /// <param name="url">The URL identifying the URL reservation</param>
        public void DeleteUrlReservation(string url)
        {
            using (var wrapper = new HttpApiWrapper())
            {
                var inputConfig = new HTTP_SERVICE_CONFIG_URLACL_SET
                {
                    KeyDesc = new HTTP_SERVICE_CONFIG_URLACL_KEY(url)
                };
                wrapper.Delete(HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                    inputConfig);
            }
        }

        /// <summary>
        /// Gets a sequence of all URL reservations on the machine
        /// </summary>
        public IEnumerable<UrlReservation> GetUrlReservations()
        {
            using (var wrapper = new HttpApiWrapper())
            {
                return wrapper.QueryMany
                (
                    HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                    token => new HTTP_SERVICE_CONFIG_URLACL_QUERY
                    {
                        dwToken = token,
                        QueryDesc = HTTP_SERVICE_CONFIG_QUERY_TYPE.HttpServiceConfigQueryNext
                    },
                    (HTTP_SERVICE_CONFIG_URLACL_SET s) => MapUrlReservation(s)
                );
            }
        }

        /// <summary>
        /// Creates UrlReservation from struct returned from native API call
        /// </summary>
        private static UrlReservation MapUrlReservation(HTTP_SERVICE_CONFIG_URLACL_SET urlSet)
        {
            string url = urlSet.KeyDesc.pUrlPrefix;
            SecurityDescriptor descriptor;
            if (!SecurityDescriptor.TryParse(urlSet.ParamDesc.pStringSecurityDescriptor, out descriptor))
            {
                descriptor = new SecurityDescriptor(Enumerable.Empty<AccessControlEntry>());
            }
            return new UrlReservation(url, descriptor);
        }
    }
}