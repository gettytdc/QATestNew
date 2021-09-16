namespace BluePrism.Api.Setup.Common
{
    public class CommandLineArguments
    {
        [CommandLineParameter("signingCertificate")]
        public string SigningCertificateFingerprint { get; set; }

        [CommandLineParameter("timestampServer")]
        public string SigningTimestampServer { get; set; }
    }
}
