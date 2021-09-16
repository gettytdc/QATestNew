namespace BluePrism.Api.Mappers.ErrorMappers
{
    using Errors;
    using Func.AspNet;
    using Models;

    public class PatchOperationErrorMapper : IErrorMapper<PatchOperationError>, IErrorMapper<NoPatchOperationsError>
    {
        public ErrorResponse GetResponseForError(PatchOperationError error, ResponseDetails configuredResponseDetails) =>
            GetErrorResponse(PatchOperationErrorReason.InvalidOperation, configuredResponseDetails);

        public ErrorResponse GetResponseForError(NoPatchOperationsError error, ResponseDetails configuredResponseDetails) =>
            GetErrorResponse(PatchOperationErrorReason.MissingOperation, configuredResponseDetails);

        private static ErrorResponse GetErrorResponse(PatchOperationErrorReason reason, ResponseDetails configuredResponseDetails) =>
            new ErrorResponse
            {
                StatusCode = configuredResponseDetails.StatusCode,
                Body = new PatchOperationErrorModel
                {
                    Message = configuredResponseDetails.Message,
                    Reason = reason,
                },
            };
    }
}
