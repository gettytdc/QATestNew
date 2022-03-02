namespace BluePrism.Api.Domain.Errors
{
    public class QueueNotFoundError : NotFoundError
    {
        public QueueNotFoundError(string errorMessage) : base(errorMessage) { }
    }
}
