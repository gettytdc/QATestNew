namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CommonTestClasses;
    using Controllers;
    using Domain;
    using SessionLogsParameters = Models.SessionLogsParameters;

    public class SessionLogsControllerClient : ControllerClientBase
    {
        public SessionLogsControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(SessionsController), openIdConfiguration) { }

        public Task<HttpResponseMessage> GetSessionLogs(Guid sessionId, string token = "") =>
            Client.SendAsync(CreateHttpRequest($"{BaseRoute}/{sessionId}/logs", HttpMethod.Get, null, token));

        public Task<HttpResponseMessage> GetSessionLogsWithParameters(
            Guid sessionId,
            SessionLogsParameters parameters,
            string token = "") =>
            Client.SendAsync(CreateHttpRequest(
                GetSessionLogsUrlWithParameters(BaseRoute, sessionId, parameters),
                HttpMethod.Get,
                null,
                token));

        public Task<HttpResponseMessage> GetSessionLogParameters(Guid sessionId, long logId) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{sessionId}/logs/{logId}/parameters",
                HttpMethod.Get,
                null,
                string.Empty
            ));

        private static string GetSessionLogsUrlWithParameters(string baseRoute, Guid sessionId, SessionLogsParameters parameters) =>
            $"{baseRoute}/{sessionId}/logs?" + QueryStringHelper.GenerateQueryStringFromParameters(parameters);
    }
}
