namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.NotFound, Message = "Session with given ID could not be found")]
    public class SessionNotFoundError : ResultError
    {
    }
}
