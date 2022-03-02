namespace BluePrism.Api.Setup.CustomActions
{
    using System.IO;
    using Microsoft.Deployment.WindowsInstaller;
    using WixSharp;
    using Common.IIS;
    using Common;
    using System.Configuration;
    using System.Web.Configuration;

    public static class CustomActions
    {
        [CustomAction]
        public static ActionResult ConfigureWebsiteCertificate(Session session) =>
            session.HandleErrors(() =>
            {
                SiteHelper.SetSiteCertificate(
                    WebAppProperties.WebAppName,
                    session.CustomActionData[MsiProperties.ApiIisSslCertificateId],
                    GetBindingInformation(
                        "*",
                        session.CustomActionData[MsiProperties.ApiIisPort],
                        session.CustomActionData[MsiProperties.ApiIisHostname]),
                    session.Log);
            });

        [CustomAction]
        public static ActionResult UpdateWebConfig(Session session) =>
            session.HandleErrors(() =>
            {
                var configurationFilePath =
                    Path.Combine(session.Property("INSTALLDIR"), "Web.config");

                var fileMap = new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = configurationFilePath
                };

                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                configuration.AppSettings.Settings["ServerName"].Value = session.CustomActionData[MsiProperties.ApiSqlServer];
                configuration.AppSettings.Settings["DatabaseName"].Value = session.CustomActionData[MsiProperties.ApiSqlDatabaseName];
                configuration.AppSettings.Settings["DatabaseUsername"].Value = session.CustomActionData[MsiProperties.ApiSqlUsername];
                configuration.AppSettings.Settings["DatabasePassword"].Value = session.CustomActionData[MsiProperties.ApiSqlPassword];
                configuration.AppSettings.Settings["UsesWindowAuth"].Value = session.CustomActionData[MsiProperties.ApiSqlAuthenticationMode] == SqlAuthenticationMode.Trusted? "true": "false";
                configuration.AppSettings.Settings["BPusername"].Value = session.CustomActionData[MsiProperties.ApiBluePrismUsername];
                configuration.AppSettings.Settings["BPpassword"].Value = session.CustomActionData[MsiProperties.ApiBluePrismPassword];
                configuration.AppSettings.Settings["Swagger.Enable"].Value = "false";

                configuration.AppSettings.SectionInformation.ProtectSection(nameof(RsaProtectedConfigurationProvider));
                configuration.AppSettings.SectionInformation.ForceSave = true;

                configuration.Save(ConfigurationSaveMode.Full);
            });

        [CustomAction]
        public static ActionResult UpdateWebsiteAppPool(Session session) =>
            session.HandleErrors(() =>
                SiteHelper.UpdateWebsiteAppPool(session.Log));

        private static string GetBindingInformation(string ipAddress, string port, string hostname) =>
            $"{ipAddress}:{port}:{hostname}";
    }
}
