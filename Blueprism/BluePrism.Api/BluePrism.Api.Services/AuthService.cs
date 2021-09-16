namespace BluePrism.Api.Services
{
    using System.Threading.Tasks;
    using BluePrism.Api.Domain.Errors;
    using BluePrism.Logging;
    using BpLibAdapters;
    using Func;

    public class AuthService : IAuthService
    {
        private readonly IAdapterAnonymousMethodRunner<IAuthServerAdapter> _authServerMethodRunner;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAdapterAnonymousMethodRunner<IAuthServerAdapter> serverAdapter,
            ILogger<AuthService> logger)
        {
            _authServerMethodRunner = serverAdapter;
            _logger = logger;
        }

        public Task<Result<string>> GetAuthServerUrl() =>
            _authServerMethodRunner.Execute(server => server.GetAuthenticationServerUrl())
                .OnError((AuthServerNotConfiguredError ex) => _logger.Info(ex.ErrorMessage));
    }
}
