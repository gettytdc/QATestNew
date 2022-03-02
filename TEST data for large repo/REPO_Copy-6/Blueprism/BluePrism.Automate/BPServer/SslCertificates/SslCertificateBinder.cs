using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using SslCertBinding.Net;

namespace BluePrism.BPServer.SslCertificates
{
    using Utilities.Functional;

    /// <summary>
    /// Utility functionality for working with SSL certificate bindings
    /// </summary>
    public class SslCertificateBinder
    {
        /// <summary>
        /// Used to cached result of check for SNI support
        /// </summary>
        private bool? _sslSniBindingsSupported;

        /// <summary>
        /// Indicates whether SSL SNI (host name) certificate bindings are supported on the current
        /// machine
        /// </summary>
        public bool SslSniBindingsSupported
        {
            get
            {
                if (_sslSniBindingsSupported == null)
                {
                    _sslSniBindingsSupported = new CertificateBindingConfiguration().SupportsSslSniBindings();
                }
                return _sslSniBindingsSupported.Value;
            }
        }

        /// <summary>
        /// Gets SSL bindings that apply to a specific port
        /// </summary>
        public IEnumerable<SslCertificateBindingInfo> GetSslBindingsForPort(int port)
        {
            var bindingConfiguration = new CertificateBindingConfiguration();
            var bindings = bindingConfiguration.Query();
            var matches = bindings.Where(x => x.EndPoint.Port == port);
            var result = MapSslBindings(matches);
            return result;
        }

        /// <summary>
        /// Adds an SSL certificate binding
        /// </summary>
        /// <param name="endPoint">The IP address or host name and port to be used by the binding</param>
        /// <param name="appId">A unique identifier for the application</param>
        /// <param name="certificate">The certificate to use for the binding</param>
        /// <param name="storeName"></param>
        public void Add(BindingEndPoint endPoint, Guid appId, X509Certificate2 certificate, StoreName storeName)
        {
            var configuration = new CertificateBindingConfiguration();
            var certificateBinding = new CertificateBinding(certificate.Thumbprint, storeName, endPoint, appId);
            configuration.Bind(certificateBinding);
        }

        /// <summary>
        /// Deletes an SSL certificate binding
        /// </summary>
        /// <param name="endPoint">The IP address or host name and port used by the binding</param>
        public void Delete(BindingEndPoint endPoint)
        {
            var configuration = new CertificateBindingConfiguration();
            configuration.Delete(endPoint);
        }

        /// <summary>
        /// Creates sequence of SslCertificateBindingInfo based on CertificateBinding objects
        /// </summary>
        private IEnumerable<SslCertificateBindingInfo> MapSslBindings(IEnumerable<CertificateBinding> bindings)
        {
            var stores = new Dictionary<string, X509Store>();
            try
            {
                return bindings.Select(binding =>
                {
                    X509Store store;
                    if (!stores.TryGetValue(binding.StoreName, out store))
                    {
                        store = new X509Store(binding.StoreName, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                        stores.Add(binding.StoreName, store);
                    }
                    var certificate = GetCertificate(store, binding);
                    return new SslCertificateBindingInfo(binding, certificate);
                });
            }
            finally
            {
                stores.Values.ForEach(x => x.Close()).Evaluate();
            }
        }

        /// <summary>
        /// Finds a certificate by its thumbprint
        /// </summary>
        private X509Certificate2 GetCertificate(X509Store store, CertificateBinding binding)
        {
            // Remove control chars from thumbprint
            string thumbprint = Regex.Replace(binding.Thumbprint, @"[^\da-fA-F]", "");
            var certificate = store.Certificates
                .Find(X509FindType.FindByThumbprint, thumbprint, false)
                .OfType<X509Certificate2>()
                .SingleOrDefault();
            return certificate;
        }
    }
}