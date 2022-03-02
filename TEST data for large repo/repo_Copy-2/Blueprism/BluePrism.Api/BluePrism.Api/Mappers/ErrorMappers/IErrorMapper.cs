namespace BluePrism.Api.Mappers.ErrorMappers
{
    using Func;
    using Func.AspNet;

    public interface IErrorMapper<in TError> where TError : ResultError
    {
        ErrorResponse GetResponseForError(TError error, ResponseDetails configuredResponseDetails);
    }
}