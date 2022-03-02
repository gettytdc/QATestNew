namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Unauthorized)]
    public class BluePrismUnauthenticatedError : ResultErrorWithMessage
    {
        public BluePrismUnauthenticatedError(string errorMessage) : base(errorMessage) { }
    }
}
