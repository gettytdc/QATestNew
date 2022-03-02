namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public class AdapterAuthenticatedMethodRunner<TAdapter> : IAdapterAuthenticatedMethodRunner<TAdapter>
        where TAdapter : IServerAdapter
    {
        private readonly IAdapterStore<TAdapter> _adapterStore;
        private readonly ITokenAccessor _tokenAccessor;

        public AdapterAuthenticatedMethodRunner(IAdapterStore<TAdapter> adapterStore, ITokenAccessor tokenAccessor)
        {
            _adapterStore = adapterStore;
            _tokenAccessor = tokenAccessor;
        }

        public Task<Result<TResult>> ExecuteForUser<TResult>(Func<TAdapter, Task<Result<TResult>>> func) =>
            _adapterStore.GetAdapterForToken(_tokenAccessor.TokenString)
                .Then(func);

        public Task<Result> ExecuteForUser(Func<TAdapter, Task<Result>> func) =>
            _adapterStore.GetAdapterForToken(_tokenAccessor.TokenString)
                .Then(func);

    }
}
