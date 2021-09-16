namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public class MockAdapterAnonymousMethodRunner<TAdapter> : IAdapterAnonymousMethodRunner<TAdapter>
        where TAdapter : IServerAdapter
    {
        private readonly TAdapter _adapter;

        public MockAdapterAnonymousMethodRunner(TAdapter adapter) => _adapter = adapter;

        public Task<Result<TResult>> Execute<TResult>(Func<TAdapter, Task<Result<TResult>>> func) => func(_adapter);
        public Task<Result> Execute(Func<TAdapter, Task<Result>> func) => func(_adapter);
    }
}
