namespace BluePrism.Api.Domain.Errors
{
    using System.Collections.Generic;
    using System.Net;
    using Func;
    using Func.AspNet;

    [ProducesStatusCode(HttpStatusCode.BadRequest)]
    public class ValidationError : ResultError
    {
        public IReadOnlyDictionary<string, string> ValidationErrors { get; }

        public ValidationError(IReadOnlyDictionary<string, string> errors) =>
            ValidationErrors = errors;
    }
}
