namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Domain;
    using Controllers;
    using CommonTestClasses;
    using Models;
    using ResourceParameters = Models.ResourceParameters;

    public class ResourcesControllerClient : ControllerClientBase
    {
        public ResourcesControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(ResourcesController), openIdConfiguration, true) {}

        public Task<HttpResponseMessage> GetResources(string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}", HttpMethod.Get, null, apiKey));

        public Task<HttpResponseMessage> GetResourcesWithParameters(ResourceParameters scheduleParameters) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?{QueryStringHelper.GenerateQueryStringFromParameters(scheduleParameters)}",
                HttpMethod.Get,
                null,
                ""
            ));

        public Task<HttpResponseMessage> GetResourcesUsingQueryString(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> ModifyResource(Guid resourceId, UpdateResourceModel updateResource, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{resourceId}", HttpMethod.Put, updateResource, apiKey));

        public Task<HttpResponseMessage> ModifyResource(string untypedResourceId, UpdateResourceModel updateResource) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{untypedResourceId}", HttpMethod.Put, updateResource));
    }
}
