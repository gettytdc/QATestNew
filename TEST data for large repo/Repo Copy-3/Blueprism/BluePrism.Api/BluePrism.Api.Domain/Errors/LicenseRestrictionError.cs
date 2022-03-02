namespace BluePrism.Api.Domain.Errors
{
    using System.Net;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.Forbidden)]
    public class LicenseRestrictionError : ResultErrorWithMessage
    {
        public LicenseRestrictionError(string errorMessage) : base(errorMessage)
        {
        }
    }
}
