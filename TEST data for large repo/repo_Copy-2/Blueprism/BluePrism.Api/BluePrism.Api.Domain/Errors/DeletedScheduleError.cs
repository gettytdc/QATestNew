namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound)]
    public class DeletedScheduleError : ResultErrorWithMessage
    {
        public DeletedScheduleError() : base("You cannot retrieve data from the deleted schedule.") { }
    }
}
