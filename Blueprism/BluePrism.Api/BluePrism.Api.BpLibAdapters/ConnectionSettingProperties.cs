namespace BluePrism.Api.BpLibAdapters
{
    using System.Security;

    public class ConnectionSettingProperties
    {
        public string ConnectionName { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUsername { get; set; }
        public SecureString DatabasePassword { get; set; }
        public bool UsesWindowAuth { get; set; }
    }
}
