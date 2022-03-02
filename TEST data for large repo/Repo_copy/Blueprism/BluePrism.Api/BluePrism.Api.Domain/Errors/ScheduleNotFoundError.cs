namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound)]
    public class ScheduleNotFoundError : ResultErrorWithMessage
    {
        public ScheduleNotFoundError() : base("A schedule with the given ID could not be found") { }
    }
}
