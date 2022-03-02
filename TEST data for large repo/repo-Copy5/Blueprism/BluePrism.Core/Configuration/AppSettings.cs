namespace BluePrism.Core.Configuration
{
    using System.Configuration;

    public class AppSettings : IAppSettings
    {
        public string this[string key] => ConfigurationManager.AppSettings[key];
    }
}