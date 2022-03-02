namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public class AdapterAnonymousMethodRunner<TAdapter> : IAdapterAnonymousMethodRunner<TAdapter>
        where TAdapter:IServerAdapter
    {
        private readonly IAdapterStore<TAdapter> _adapterStore;

        public AdapterAnonymousMethodRunner(IAdapterStore<TAdapter> adapterStore)
        {
            _adapterStore = adapterStore;
        }
        public Task<Result<TResult>> Execute<TResult>(Func<TAdapter, Task<Result<TResult>>> func) =>
            _adapterStore.GetAnonymousAdapter()
                .Then(func);
    }
}
