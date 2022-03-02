namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Domain;
    using Controllers;

    public class EncryptionSchemesControllerClient : ControllerClientBase
    {
        public EncryptionSchemesControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(EncryptionSchemesController), openIdConfiguration) { }

        public Task<HttpResponseMessage> GetEncryptionSchemes(string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}", HttpMethod.Get, null, apiKey));
    }
}
