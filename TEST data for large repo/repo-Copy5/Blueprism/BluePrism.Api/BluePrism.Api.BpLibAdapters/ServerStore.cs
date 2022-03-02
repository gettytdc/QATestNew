namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Concurrent;
    using AutomateAppCore;
    using Domain.Errors;
    using Func;

    using static Func.ResultHelper;

    public class ServerStore : IServerStore
    {
        private readonly IUserStaticMethodWrapper _userStaticMethodWrapper;
        private readonly IBluePrismServerFactory _serverFactory;
        private readonly ConcurrentDictionary<string, IServer> _serverInstances = new ConcurrentDictionary<string, IServer>();
        
        public ServerStore(
            IUserStaticMethodWrapper userStaticMethodWrapper,
            IBluePrismServerFactory bluePrismServerFactory)
        {
            _userStaticMethodWrapper = userStaticMethodWrapper;
            _serverFactory = bluePrismServerFactory;
        }

        public Result<IServer> GetUnkeyedServerInstance() => Succeed(_serverFactory.ClientInit());

        public Result<IServer> GetServerInstanceForToken(string token)
        {
            var error = string.Empty;
            var server = _serverInstances.GetOrAdd(token, x => CreateServerForToken(x, out error));

            if (server == null)
            {
                _serverInstances.TryRemove(token, out _);
                return ResultHelper<IServer>.Fail(new BluePrismUnauthenticatedError(error));
            }

            return Succeed(server);
        }

        public void CloseServerInstance(string token) =>
            _serverInstances.TryRemove(token, out _);

        private IServer CreateServerForToken(string token, out string error)
        {
            try
            {
                var server = _serverFactory.ClientInit();
                var loginResult = _userStaticMethodWrapper.LoginWithAccessToken("API", token, "en-us", server);

                if (!loginResult.IsSuccess)
                {
                    error = $"Blue Prism login error: {loginResult.Description}";
                    return null; // Required to support atomic GetOrAdd method
                }

                error = default(string);
                return server;
            }
            catch(Exception ex)
            {
                error = $"Blue Prism login error: {ex})";
                return null;
            }
        }
    }
}
