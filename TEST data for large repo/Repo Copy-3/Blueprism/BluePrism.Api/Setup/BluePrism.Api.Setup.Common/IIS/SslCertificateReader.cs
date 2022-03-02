namespace BluePrism.Api.Setup.Common.IIS
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;

    public static class SslCertificateReader
    {
        private static readonly Lazy<IList<InstallerCertificate>> InstallerCertificates =
            new Lazy<IList<InstallerCertificate>>(GetSslCertsLocalMachine);

        public static IList<InstallerCertificate> GetInstallerCertificates() =>
            InstallerCertificates.Value;

        public static InstallerCertificate GetCertificateOrThrow(string certificateId) =>
            InstallerCertificates.Value
                .SingleOrDefault(x => x.Id.Equals(certificateId, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("No matching certificate found with selected certificate");

        private static IList<InstallerCertificate> GetSslCertsLocalMachine()
        {
            Func<X509Certificate2, (string Store, X509Certificate2 Certificate)> GetCertificate(string store) =>
                certificate => (store, certificate);

            using (var personalStore = new X509Store("My", StoreLocation.LocalMachine))
            using (var webHostingStore = new X509Store("WebHosting", StoreLocation.LocalMachine))
            {
                personalStore.Open(OpenFlags.ReadOnly);
                webHostingStore.Open(OpenFlags.ReadOnly);

                var personalCertificates = personalStore.Certificates.OfType<X509Certificate2>().Select(GetCertificate("My"));
                var webHostingCertificates = webHostingStore.Certificates.OfType<X509Certificate2>().Select(GetCertificate("WebHosting"));

                return personalCertificates.Concat(webHostingCertificates)
                    .Select(x => new InstallerCertificate(x.Certificate, x.Store))
                    .ToList();
            }
        }
    }
}
