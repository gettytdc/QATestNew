namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Func;

    using static Func.OptionHelper;
    using static Func.ResultHelper;

    public class AdapterStore<TAdapter> : IAdapterStore<TAdapter>, IDisposable
        where TAdapter : IServerAdapter
    {
        private Dictionary<string, SessionItem<TAdapter>> _adapters = new Dictionary<string, SessionItem<TAdapter>>();
        private readonly Timer _cleanupTimer;
        private readonly Func<IServer, TAdapter> _adapterFactory;
        private readonly IServerStore _serverStore;

        public AdapterStore(
            Func<IServer, TAdapter> adapterFactory,
            IServerStore serverStore)
        {
            _cleanupTimer = new Timer(CleanSessions, null, 30000, 30000);
            _adapterFactory = adapterFactory;
            _serverStore = serverStore;
        }

        public Task<Result<TAdapter>> GetAdapterForToken(string token) =>
            GetOrCreateSession(token)
                .Then(RefreshSessionExpiry(token))
                .ThenMap(x => x.Adapter);

        public Task<Result<TAdapter>> GetAnonymousAdapter() => Task.Run(() => CreateUnauthenticatedSession().ThenMap(x => x.Adapter));

        private Task<Result<SessionItem<TAdapter>>> GetOrCreateSession(string token) => Task.Run(() =>
        {
            lock (_adapters)
            {
                return
                    _adapters.ContainsKey(token) && _adapters[token].ExpiresTick > DateTime.UtcNow.Ticks
                        ? Succeed(_adapters[token])
                        : CreateAdapterForToken(token);
            }
        });

        private Func<SessionItem<TAdapter>, Result<SessionItem<TAdapter>>> RefreshSessionExpiry(string token) => session =>
        {
            lock (_adapters)
            {
                _adapters[token] = session.SetExpiry(DateTime.UtcNow.AddMinutes(30).Ticks);
            }

            return Succeed(session);
        };

        private Result<SessionItem<TAdapter>> CreateAdapterForToken(string token) =>
            CreateAdapter(() => _serverStore.GetServerInstanceForToken(token));

        private Result<SessionItem<TAdapter>> CreateUnauthenticatedSession() =>
            CreateAdapter(() => _serverStore.GetUnkeyedServerInstance());

        private Result<SessionItem<TAdapter>> CreateAdapter(Func<Result<IServer>> serverFactory) =>
             serverFactory()
                 .ThenMap(CreateAdapterWithServer);

        private SessionItem<TAdapter> CreateAdapterWithServer(IServer server) =>
            _adapterFactory(server)
                .Map(adapter => new SessionItem<TAdapter>(server, adapter, 0));

        private void CleanSessions(object _)
        {
            lock (_adapters)
            {
                _adapters = _adapters
                    .Select(CloseSessionIfExpired)
                    .OfType<Some<KeyValuePair<string, SessionItem<TAdapter>>>>()
                    .ToDictionary(x => x.Value.Key, x => x.Value.Value);
            }
        }

        private Option<KeyValuePair<string, SessionItem<TAdapter>>> CloseSessionIfExpired(KeyValuePair<string, SessionItem<TAdapter>> item)
        {
            if (item.Value.ExpiresTick > DateTime.UtcNow.Ticks)
                return Some(item);

            item.Value.Server.Logout();
            _serverStore.CloseServerInstance(item.Key);
            return None<KeyValuePair<string, SessionItem<TAdapter>>>();
        }

        public void Dispose() =>
            _cleanupTimer.Dispose();
    }
}
