using System;
using System.Security.Cryptography.X509Certificates;
using SslCertBinding.Net;

namespace BluePrism.BPServer.SslCertificates
{
    /// <summary>
    /// Contains information about an SSL certificate binding and the certificate used for 
    /// display within UI
    /// </summary>
    public class SslCertificateBindingInfo
    {
        public SslCertificateBindingInfo(CertificateBinding binding, X509Certificate2 certificate)
        {
            EndPoint = binding.EndPoint;
            Thumbprint = binding.Thumbprint;
            StoreName = binding.StoreName;
            Certificate = certificate;
        }

        /// <summary>
        /// The address and port of the endpoint binding
        /// </summary>
        public BindingEndPoint EndPoint { get; private set; }
            
        /// <summary>
        /// Gets a string combining the IP address or host name followed by the
        /// port separated by a colon character
        /// </summary>
        public string EndPointAddressAndPort 
        {
            get { return EndPoint.AddressAndPort; } 
        }

        /// <summary>
        /// The certificate used by the binding
        /// </summary>
        public X509Certificate2 Certificate { get; private set; }
        
        /// <summary>
        /// The certificate thumbprint
        /// </summary>
        public string Thumbprint { get; private set; }

        /// <summary>
        /// The name of the certificate store
        ///  </summary>
        public string StoreName { get; private set; }

        /// <summary>
        /// Information about the certificate to display in listing
        /// </summary>
        public string CertificateDescription
        {
            get
            {
                string description;
                if (Certificate != null)
                {
                    return String.Format("{0} - Expiry: {1}, Issuer: {2}, Store: {3}, Thumbprint: {4}", Certificate.SubjectName.Name,
                        Certificate.GetExpirationDateString(), Certificate.Issuer, 
                        StoreName,
                        Certificate.Thumbprint);
                }
                else
                {
                    description = "No certificate found with thumbprint: " + Thumbprint;
                }
                return description;
            }
        }
    }
}