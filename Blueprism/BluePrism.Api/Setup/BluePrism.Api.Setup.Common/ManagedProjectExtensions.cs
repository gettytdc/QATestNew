namespace BluePrism.Api.Setup.Common
{
    using System;
    using WixSharp;

    public static class ManagedProjectExtensions
    {
        public static void ApplyDigitalSigning(this ManagedProject @this,
            string certificateId, string signingTimestampServer, string description) =>
            @this.DigitalSignature = new DigitalSignature
            {
                CertificateId = certificateId,
                CertificateStore = StoreType.sha1Hash,
                HashAlgorithm = HashAlgorithmType.sha256,
                Description = description,
                TimeUrl = new Uri(signingTimestampServer),
            };
    }
}
