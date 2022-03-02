namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class ScheduleNotRetiredError : ResultErrorWithMessage
    {
        public ScheduleNotRetiredError() : base("The given schedule is not currently retired")
        {
        }
    }
}
