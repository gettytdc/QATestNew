namespace BluePrism.Api.Errors
{
    using System.Net;
    using Func;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.BadRequest)]
    [MessageTextSource(nameof(Message))]
    public class EmptyCollectionError : ResultError
    {
        public string Message { get; }
        public EmptyCollectionError(string message) => Message = message;
    }
}
