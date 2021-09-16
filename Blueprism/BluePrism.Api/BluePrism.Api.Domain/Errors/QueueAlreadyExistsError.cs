namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class QueueAlreadyExistsError : ResultErrorWithMessage
    {
        public QueueAlreadyExistsError() : base("A queue with this name already exists") { }
    }
}
