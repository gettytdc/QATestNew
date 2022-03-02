namespace BluePrism.Api.BpLibAdapters
{
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Api.Domain.Errors;
    using BluePrism.Server.Domain.Models;
    using Func;

    using static ServerResultTask;

    public class AuthServerAdapter : IAuthServerAdapter
    {
        private readonly IServer _server;

        public AuthServerAdapter(IServer server) => _server = server;

        public async Task<Result<string>> GetAuthenticationServerUrl() =>
            await RunOnServer(() => _server.GetAuthenticationServerUrl())
                .Catch<AuthenticationServerNotConfiguredException>(_ => ResultHelper<string>.Fail(new AuthServerNotConfiguredError()))
                .Execute();
    }
}
