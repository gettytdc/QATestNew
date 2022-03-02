namespace BluePrism.Api.Domain.Errors
{
    using Func;
    using Func.AspNet;

    [MessageTextSource(nameof(ErrorMessage))]
    public abstract class ResultErrorWithMessage : ResultError
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public string ErrorMessage { get; }

        protected ResultErrorWithMessage(string errorMessage) => ErrorMessage = errorMessage;
    }
}
