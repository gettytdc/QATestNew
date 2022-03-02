using BluePrism.Common.Security;
using System;

namespace BluePrism.Core.Encryption
{
    /// <summary>
    /// Service class to wrap access to the certificate information
    /// </summary>
    public class CertificateServices
    {
        private readonly ICertificateStoreService _certificateStoreService;
        public CertificateServices(ICertificateStoreService certificateStoreService)
        {
            _certificateStoreService = certificateStoreService ?? throw new ArgumentNullException(nameof(certificateStoreService));
        }
        public CertificateServices() : this(new CertificateStoreService())
        {
        }

        /// <summary>
        /// Based on the thumbprint provided, return the expiry date of a given certificate.
        /// </summary>
        /// <param name="thumbprint">thumbprint of the cert</param>
        /// <returns></returns>
        public DateTime CertificateExpiryDateTime(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint))
            {
                throw new ArgumentException($"{nameof(thumbprint)} is null or empty", nameof(thumbprint));
            }
            //this will throw if the certificate can't be found.
            using (var certificate = _certificateStoreService.GetCertificate(thumbprint))
            {
                return DateTime.Parse(certificate.GetExpirationDateString());
            }
        }
    }
}
