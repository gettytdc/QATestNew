namespace BluePrism.Api.Domain.Errors
{
    public class AuthServerNotConfiguredError : ResultErrorWithMessage
    {
        public AuthServerNotConfiguredError() : base("Authentication server has not been configured.") { }
    }
}
