namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound)]
    public class QueueItemNotFoundError : ResultErrorWithMessage
    {
        public QueueItemNotFoundError() : base("A queue item with the given ID could not be found") { }
    }
}
