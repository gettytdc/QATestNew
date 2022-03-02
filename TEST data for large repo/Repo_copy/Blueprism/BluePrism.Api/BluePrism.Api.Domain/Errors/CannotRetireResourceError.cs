namespace BluePrism.Api.Domain.Errors
{
    public class CannotRetireResourceError : ResultErrorWithMessage
    {
        public CannotRetireResourceError(string errorMessage) : base(errorMessage)
        {
        }
    }
}
