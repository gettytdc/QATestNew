namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class QueueNotEmptyError : ResultErrorWithMessage
    {
        public QueueNotEmptyError(string message) : base(message) { }
    }
}
