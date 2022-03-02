namespace BluePrism.Api.Errors
{
    using System.Net;
    using Func;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.BadRequest)]
    [MessageTextSource(nameof(Message))]
    public class PatchOperationError : ResultError
    {
        public string Message { get; }

        public PatchOperationError(string message) => Message = message;
    }
}
