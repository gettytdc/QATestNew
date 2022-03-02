namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.BadRequest)]
    public class InvalidArgumentsError : ResultErrorWithMessage
    {
        public InvalidArgumentsError(string errorMessage) : base(errorMessage)
        {
        }
    }
}
