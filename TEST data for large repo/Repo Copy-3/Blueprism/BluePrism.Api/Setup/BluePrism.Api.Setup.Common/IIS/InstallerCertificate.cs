namespace BluePrism.Api.Setup.Common.IIS
{
    using System.Security.Cryptography.X509Certificates;
    using static System.String;

    public class InstallerCertificate
    {
        public string CertificateDisplayName { get; }
        public byte[] CertificateHash { get; }
        public string CertificateStoreName { get; }
        public string Id { get; }

        public InstallerCertificate(X509Certificate2 certificate, string certificateStoreName)
        {
            CertificateDisplayName = GetCertificateIisName(certificate);
            CertificateHash = certificate.GetCertHash();
            CertificateStoreName = certificateStoreName;
            Id = certificate.Thumbprint;
        }

        private static string GetCertificateIisName(X509Certificate2 certificate) =>
            IsNullOrEmpty(certificate.FriendlyName)
                ? certificate.GetNameInfo(X509NameType.SimpleName, false)
                : certificate.FriendlyName;
    }
}
