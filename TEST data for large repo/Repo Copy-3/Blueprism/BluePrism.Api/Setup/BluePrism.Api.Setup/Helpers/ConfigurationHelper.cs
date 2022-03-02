namespace BluePrism.Api.Setup.Helpers
{
    using System.Collections.Generic;
    using Common;
    using global::WixSharp;

    public static class ConfigurationHelper
    {
        private const string DefaultPropertyValue = "!!bp-default-value!!";

        public static void SetProperties(SetupEventArgs setupEventArgs)
        {
            void SetPropertyIfDefault(string propertyName, string value)
            {
                if (setupEventArgs.Session[propertyName] == DefaultPropertyValue)
                {
                    setupEventArgs.Session[propertyName] = value;
                }
            }

            SetPropertyIfDefault(MsiProperties.ApiIisHostname, "localhost");
            SetPropertyIfDefault(MsiProperties.ApiIisPort, "443");
            SetPropertyIfDefault(MsiProperties.ApiIisSslCertificateId, "");

            SetPropertyIfDefault(MsiProperties.ApiSqlServer, "");
            SetPropertyIfDefault(MsiProperties.ApiSqlDatabaseName, "");
            SetPropertyIfDefault(MsiProperties.ApiSqlAuthenticationMode, SqlAuthenticationMode.Trusted);
            SetPropertyIfDefault(MsiProperties.ApiSqlUsername, "");
            SetPropertyIfDefault(MsiProperties.ApiSqlPassword, "");

            SetPropertyIfDefault(MsiProperties.ApiBluePrismUsername, "");
            SetPropertyIfDefault(MsiProperties.ApiBluePrismPassword, "");

            SetPropertyIfDefault(MsiProperties.Locale, "en-US");
        }

        public static List<Property> GetProperties() =>
            new List<Property>
            {
                new Property(MsiProperties.ApiIisHostname, DefaultPropertyValue),
                new Property(MsiProperties.ApiIisPort, DefaultPropertyValue),
                new Property(MsiProperties.ApiIisSslCertificateId, DefaultPropertyValue),

                new Property(MsiProperties.ApiSqlServer, DefaultPropertyValue),
                new Property(MsiProperties.ApiSqlDatabaseName, DefaultPropertyValue),
                new Property(MsiProperties.ApiSqlAuthenticationMode, DefaultPropertyValue),
                new Property(MsiProperties.ApiSqlUsername, DefaultPropertyValue),
                new Property(MsiProperties.ApiSqlPassword, DefaultPropertyValue),

                new Property(MsiProperties.ApiBluePrismUsername, DefaultPropertyValue),
                new Property(MsiProperties.ApiBluePrismPassword, DefaultPropertyValue),

                new Property(MsiProperties.Locale, DefaultPropertyValue),
            };
    }
}
