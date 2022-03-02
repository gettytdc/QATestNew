namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CommonTestClasses;
    using Domain;
    using Controllers;
    using Func;
    using SessionParameters = Models.SessionParameters;

    public class SessionsControllerClient : ControllerClientBase
    {
        public SessionsControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(SessionsController), openIdConfiguration) { }

        public Task<HttpResponseMessage> GetSessionsUsingQueryString(string queryString, string apiKey = "") =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?{queryString}",
                HttpMethod.Get,
                null,
                apiKey));

        public Task<HttpResponseMessage> GetSessions(
            string pagingToken = null,
            int itemsPerPage = 10,
            SessionParameters parameters = null) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?PagingToken={pagingToken}&itemsPerPage={itemsPerPage}&{parameters?.Map(QueryStringHelper.GenerateQueryStringFromParameters) ?? ""}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetSession(Guid sessionId) =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{sessionId}", HttpMethod.Get, null, ""));
    }
}
