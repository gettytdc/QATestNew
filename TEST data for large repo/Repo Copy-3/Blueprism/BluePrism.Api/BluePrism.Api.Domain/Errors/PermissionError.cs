namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Forbidden)]
    public class PermissionError : ResultErrorWithMessage
    {
        public PermissionError(string errorMessage) : base(errorMessage)
        {
        }
    }
}
