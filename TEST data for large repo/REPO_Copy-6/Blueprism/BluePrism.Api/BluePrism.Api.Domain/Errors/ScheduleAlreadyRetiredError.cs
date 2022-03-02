namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class ScheduleAlreadyRetiredError : ResultErrorWithMessage
    {
        public ScheduleAlreadyRetiredError() : base("The given schedule is already retired")
        {
        }
    }
}
