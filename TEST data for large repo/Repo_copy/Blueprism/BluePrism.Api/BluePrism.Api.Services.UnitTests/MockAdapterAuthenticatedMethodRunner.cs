namespace BluePrism.Api.Services.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public class MockAdapterAuthenticatedMethodRunner<TAdapter> : IAdapterAuthenticatedMethodRunner<TAdapter>
        where TAdapter : IServerAdapter
    {
        private readonly TAdapter _adapter;

        public MockAdapterAuthenticatedMethodRunner(TAdapter adapter) => _adapter = adapter;

        public Task<Result<TResult>> ExecuteForUser<TResult>(Func<TAdapter, Task<Result<TResult>>> func) => func(_adapter);
        public Task<Result> ExecuteForUser(Func<TAdapter, Task<Result>> func) => func(_adapter);
    }
}
