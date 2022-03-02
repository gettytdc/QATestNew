using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace BluePrism.Core
{
    public class TrustedCertificateValidator
        : X509CertificateValidator
    {
        private readonly IReadOnlyCollection<string> _trustedCertificates;


        public TrustedCertificateValidator(IEnumerable<X509Certificate2> trustedCerts)
        {
            if (trustedCerts is null)
            {
                throw new ArgumentNullException(nameof(trustedCerts));
            }

            _trustedCertificates = new List<string>(trustedCerts.Select(x => x.Thumbprint));
        }


        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate is null)
                throw new ArgumentNullException(nameof(certificate));
            
            if(!_trustedCertificates.Any(x => x == certificate.Thumbprint))
            {
                throw new SecurityException("unknown certificate");
            }
        }
    }
}
