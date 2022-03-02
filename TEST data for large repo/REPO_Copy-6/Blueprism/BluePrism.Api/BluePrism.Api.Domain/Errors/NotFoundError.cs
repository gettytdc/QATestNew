namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound)]
    public class NotFoundError : ResultErrorWithMessage
    {
        public NotFoundError(string errorMessage) : base(errorMessage) { }
    }
}
