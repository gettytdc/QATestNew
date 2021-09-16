namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class NotOnlyOneQueueUpdatedError : ResultErrorWithMessage
    {
        public NotOnlyOneQueueUpdatedError(string errorMessage) : base(errorMessage)
        {
        }
    }
}
