namespace BluePrism.Api.Mappers.ErrorMappers
{
    using System.Linq;
    using Domain.Errors;
    using Func.AspNet;
    using Models;

    public class ValidationErrorMapper : IErrorMapper<ValidationError>
    {
        public ErrorResponse GetResponseForError(ValidationError error, ResponseDetails configuredResponseDetails) =>
            new ErrorResponse
            {
                StatusCode = configuredResponseDetails.StatusCode,
                Body = error.ValidationErrors
                    .Select(x => new ValidationErrorModel { InvalidField = x.Key, Message = x.Value })
            };
    }
}
