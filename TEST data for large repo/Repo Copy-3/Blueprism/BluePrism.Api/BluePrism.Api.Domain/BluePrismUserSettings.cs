namespace BluePrism.Api.Domain
{
    using System.Security;

    public class BluePrismUserSettings
    {
        public string Username { get; set; }
        public SecureString Password { get; set; }
    }
}
