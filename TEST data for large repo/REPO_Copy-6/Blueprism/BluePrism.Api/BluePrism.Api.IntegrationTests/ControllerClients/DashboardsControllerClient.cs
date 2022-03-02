namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Controllers;
    using Domain;

    public class DashboardsControllerClient : ControllerClientBase
    {
        public DashboardsControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(DashboardsController), openIdConfiguration ) { }

        public Task<HttpResponseMessage> GetWorkQueueCompositions(IReadOnlyCollection<Guid> workQueueIds) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/workQueueCompositions?workQueueIds={string.Join("&workQueueIds=", workQueueIds)}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetWorkQueueCompositionsUsingQueryString(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/workQueueCompositions?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public  Task<HttpResponseMessage>  GetResourceUtilization(Models.ResourceUtilizationParameters resourceUtilizationParameters) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/resourceUtilization?startDate={resourceUtilizationParameters.StartDate.Date}{string.Join("&resourceIds=", resourceUtilizationParameters.ResourceIds)}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetResourceUtilization(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/resourceUtilization?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetResourcesSummaryUtilization(Models.ResourcesSummaryUtilizationParameters resourcesSummaryUtilizationParameters) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/resourcesSummaryUtilization?startDate={resourcesSummaryUtilizationParameters.StartDate.Date}&endDate={resourcesSummaryUtilizationParameters.EndDate.Date}{string.Join("&resourceIds=", resourcesSummaryUtilizationParameters.ResourceIds)}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetResourcesSummaryUtilization(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/resourcesSummaryUtilization?{queryString}",
                HttpMethod.Get,
                null,
                ""));
    }
}
