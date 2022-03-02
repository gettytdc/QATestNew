namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Conflict)]
    public class ResourceNotRetiredError : ResultErrorWithMessage
    {
        public ResourceNotRetiredError() : base("The given resource is not currently retired")
        {
        }
    }
}
