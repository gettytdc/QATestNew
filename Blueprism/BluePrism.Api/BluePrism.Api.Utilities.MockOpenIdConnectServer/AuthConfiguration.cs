namespace BluePrism.Api.Utilities.MockOpenIdConnectServer
{
    // This is horrible but it's only in a utility so seems tolerable
    public static class AuthConfiguration
    {
        public static string Audience { get; set; }
        public static string ClientId { get; set; }
    }
}
