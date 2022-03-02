using System;
using System.ServiceModel;
using BluePrism.ClientServerResources.Core.Config;
using BluePrism.Common.Security;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;

namespace BluePrism.ClientServerResources.Wcf
{
    public static class CredentialFactory
    {

        public static void CreateCertificateCredentials(ref ServiceHost shost, ConnectionConfig config)
        {
            // passing ServiceHost by reference is necessary due to it's readonly credentials
            if (shost is null)
                throw new ArgumentNullException(nameof(shost));
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            var store = new CertificateStoreService();

            var clientCert = store.GetCertificateByName(
                config.ClientCertificateName,
                config.ClientStore);

            shost.Credentials.ClientCertificate.Authentication.CertificateValidationMode
                = X509CertificateValidationMode.Custom;
            shost.Credentials.ClientCertificate.Authentication.CustomCertificateValidator
                = new BluePrism.Core.TrustedCertificateValidator(new[] { clientCert });
            shost.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                config.ServerStore,
                X509FindType.FindBySubjectName,
                config.CertificateName);
        }
    }
}
