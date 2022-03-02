namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Controllers;
    using Domain;
    using Models;

    public class ScheduleSessionsControllerClient : ControllerClientBase
    {
        public ScheduleSessionsControllerClient(HttpClient client, OpenIdConfiguration openIdConfiguration) : base(client, typeof(SchedulesController), openIdConfiguration) { }

        public Task<HttpResponseMessage> ScheduleOneOffScheduleRun(int scheduleId, ScheduleOneOffScheduleRunModel model) =>
            ScheduleOneOffScheduleRun(scheduleId, (object)model);

        public Task<HttpResponseMessage> ScheduleOneOffScheduleRun(int scheduleId, object model) =>
            Client.SendAsync(CreateHttpRequest(
                $"{BaseRoute}/{scheduleId}/sessions/",
                HttpMethod.Post,
                model,
                ""));
    }
}
