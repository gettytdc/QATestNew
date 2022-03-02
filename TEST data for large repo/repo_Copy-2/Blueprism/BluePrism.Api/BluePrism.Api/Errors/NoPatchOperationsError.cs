namespace BluePrism.Api.Errors
{
    using System.Net;
    using Func;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.BadRequest)]
    [MessageTextSource(nameof(Message))]
    public class NoPatchOperationsError : ResultError
    {
        public string Message => "No patch operations have been specified";
    }
}
