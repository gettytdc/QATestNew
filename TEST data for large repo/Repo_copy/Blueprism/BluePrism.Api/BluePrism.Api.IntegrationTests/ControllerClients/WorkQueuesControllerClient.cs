namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CommonTestClasses;
    using Controllers;
    using Domain;
    using Models;
    using WorkQueueItemParameters = Models.WorkQueueItemParameters;
    using WorkQueueParameters = Models.WorkQueueParameters;

    public class WorkQueuesControllerClient : ControllerClientBase
    {
        public WorkQueuesControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(WorkQueuesController), openIdConfiguration) { }

        public Task<HttpResponseMessage> GetWorkQueuesNoParameters(string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}", HttpMethod.Get, null, apiKey));

        public Task<HttpResponseMessage> GetWorkQueuesSortBy(string sortBy, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}?sortBy={sortBy}", HttpMethod.Get, null, apiKey));

        public Task<HttpResponseMessage> DeleteQueue(Guid queueId, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{queueId}", HttpMethod.Delete, null, apiKey));

        public Task<HttpResponseMessage> UpdateWorkQueue(Guid queueId, object data, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{queueId}", new HttpMethod("PATCH"), data, apiKey));

        public Task<HttpResponseMessage> CreateQueue(CreateWorkQueueRequestModel requestModel) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}", HttpMethod.Post, requestModel, ""));

        public Task<HttpResponseMessage> GetWorkQueueItem(Guid workQueueItemId) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/items/{workQueueItemId}", HttpMethod.Get, null, ""));

        public Task<HttpResponseMessage> WorkQueueGetQueueItems(Guid workQueueId, WorkQueueItemSortBy sortBy, int itemsPerPage = 10) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{workQueueId}/items?workQueueItemParameters.sortBy={sortBy}&workQueueItemParameters.itemsPerPage={itemsPerPage}", HttpMethod.Get, null, ""));

        public Task<HttpResponseMessage> GetWorkQueuesUsingQueryString(string queryString, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest( $"{BaseRoute}?{queryString}",HttpMethod.Get,null, apiKey));

        public Task<HttpResponseMessage> GetWorkQueueWithFiltersParameters(WorkQueueParameters parameters, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest(GetWorkQueueUrlWithParameters(BaseRoute, parameters), HttpMethod.Get, null, apiKey));

        private static string GetWorkQueueUrlWithParameters(string baseRoute, WorkQueueParameters parameters) =>
            $"{baseRoute}?" + QueryStringHelper.GenerateQueryStringFromParameters(parameters);

        public Task<HttpResponseMessage> GetWorkQueueItemsUsingQueryString(Guid workQueueId, string queryString, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{workQueueId}/items?{queryString}",
                HttpMethod.Get,
                null,
                apiKey));

        public Task<HttpResponseMessage> GetWorkQueueItemsWithFiltersParameters(
            Guid workQueueId,
            WorkQueueItemParameters parameters,
            string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest(
                GetWorkQueueItemsUrlWithParameters(BaseRoute, workQueueId, parameters),
                HttpMethod.Get,
                null,
                apiKey));

        public Task<HttpResponseMessage> CreateWorkQueueItems(Guid workQueueId, IEnumerable<CreateWorkQueueItemModel> requestModel) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{workQueueId}/items", HttpMethod.Post, requestModel, ""));

        private static string GetWorkQueueItemsUrlWithParameters(string baseRoute, Guid workQueueId, WorkQueueItemParameters parameters) =>
            $"{baseRoute}/{workQueueId}/items?" + QueryStringHelper.GenerateQueryStringFromParameters(parameters);

        public Task<HttpResponseMessage> GetWorkQueue(Guid workQueueId) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{workQueueId}", HttpMethod.Get, null, ""));
    }
}
