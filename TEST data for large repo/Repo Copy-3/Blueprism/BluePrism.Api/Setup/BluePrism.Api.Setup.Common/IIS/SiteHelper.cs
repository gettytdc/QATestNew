namespace BluePrism.Api.Setup.Common.IIS
{
    using System;
    using System.Linq;
    using Microsoft.Web.Administration;

    public static class SiteHelper
    {
        public delegate void LogMethod(string message);

        public static void SetSiteCertificate(string siteName, string certificateId, string bindingInformation, LogMethod log)
        {
            log("Setting certificate for API");

            using (var serverManager = new ServerManager())
            {
                var site = serverManager.GetSiteByName(siteName);
                site.Bindings.Clear();
                var certificate = SslCertificateReader.GetCertificateOrThrow(certificateId);
                site.Bindings.Add(bindingInformation, certificate.CertificateHash, certificate.CertificateStoreName);

                serverManager.CommitChanges();
            }
        }

        public static void UpdateWebsiteAppPool(LogMethod log)
        {
            using (var serverManager = new ServerManager())
            {
                if (serverManager.ApplicationPools.Any(pool => pool.Name == WebAppProperties.AppPoolName))
                {
                    try
                    {
                        var website = serverManager.GetSiteByName(WebAppProperties.WebAppName);

                        website.Stop();
                        website.ApplicationDefaults.ApplicationPoolName = WebAppProperties.AppPoolName;
                        serverManager.CommitChanges();
                        website.Start();
                    }
                    catch (Exception ex)
                    {
                        log("Unable to update website app pool: " + ex.Message);
                    }
                }
            }
        }

        private static Site GetSiteByName(this ServerManager server, string siteName) =>
            server.Sites.SingleOrDefault(s => s.Name == siteName)
            ?? throw new ArgumentException($"Could not find website {siteName}!", nameof(siteName));
    }
}
