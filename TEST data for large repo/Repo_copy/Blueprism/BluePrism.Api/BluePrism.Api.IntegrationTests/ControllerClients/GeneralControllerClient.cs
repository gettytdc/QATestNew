namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Domain;
    using Controllers;

    public class GeneralControllerClient : ControllerClientBase
    {
        public GeneralControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(ResourcesController), openIdConfiguration, false) { }

        public Task<HttpResponseMessage> MakeRequestThatReturnsHttpNoContent(Guid id, object data, string token = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{id}", HttpMethod.Put, data, token));

        public Task<HttpResponseMessage> MakeRequestToAuthorizeEndpoint(string token = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}", HttpMethod.Get, null, token));
    }
}
