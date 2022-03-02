namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System.Threading.Tasks;
    using System.Net.Http;
    using CommonTestClasses;
    using Controllers;
    using Domain;
    using ScheduleLogParameters = Models.ScheduleLogParameters;
    using ScheduleParameters = Models.ScheduleParameters;

    public class SchedulesControllerClient : ControllerClientBase
    {
        public SchedulesControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(SchedulesController), openIdConfiguration, true) { }

        public Task<HttpResponseMessage> GetSchedulesWithParameters(ScheduleParameters scheduleParameters) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?{QueryStringHelper.GenerateQueryStringFromParameters(scheduleParameters)}",
                HttpMethod.Get,
                null,
                ""
                ));

        public Task<HttpResponseMessage> GetSchedulesUsingQueryString(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleByScheduleId(int scheduleId) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}",
                HttpMethod.Get,
                null,
                ""
            ));

        public Task<HttpResponseMessage> GetScheduleLogsByScheduleId(int scheduleId, int itemsPerPage) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}/logs?scheduleLogParameters.ItemsPerPage={itemsPerPage}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleLogsByScheduleId(string scheduleId, string itemsPerPage) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}/logs?scheduleLogParameters.ItemsPerPage={itemsPerPage}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleLogsByScheduleIdUsingScheduleIdAndQueryString(int scheduleId, string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}/logs?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleLogs(int itemsPerPage) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/logs?scheduleLogParameters.ItemsPerPage={itemsPerPage}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleLogs(string itemsPerPage) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/logs?scheduleLogParameters.ItemsPerPage={itemsPerPage}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduleLogsWithParameters(ScheduleLogParameters scheduleLogParameters) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/logs?{QueryStringHelper.GenerateQueryStringFromParameters(scheduleLogParameters)}",
                HttpMethod.Get,
                null,
                ""
            ));

        public Task<HttpResponseMessage> GetScheduleLogsUsingScheduleIdAndQueryString(string queryString) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/logs?{queryString}",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> UpdateSchedule(string scheduleId, object data) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}",
                HttpMethod.Put,
                data,
                ""));

        public Task<HttpResponseMessage> GetScheduledTasks(int scheduleId) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}/tasks",
                HttpMethod.Get,
                null,
                ""));

        public Task<HttpResponseMessage> GetScheduledSessions(int taskId) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/tasks/{taskId}/sessions",
                HttpMethod.Get,
                null,
                ""));
    }
}
