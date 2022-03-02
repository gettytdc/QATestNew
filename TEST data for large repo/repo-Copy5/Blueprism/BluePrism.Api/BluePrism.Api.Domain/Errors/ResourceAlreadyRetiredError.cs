namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class ResourceAlreadyRetiredError : ResultErrorWithMessage
    { 
        public ResourceAlreadyRetiredError() : base("The given resource is already retired")
        {
        }
    }
}
