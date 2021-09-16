namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound)]
    public class TaskNotFoundError : ResultErrorWithMessage
    {
        public TaskNotFoundError() : base("A task with the given ID could not be found") { }
    }
}
