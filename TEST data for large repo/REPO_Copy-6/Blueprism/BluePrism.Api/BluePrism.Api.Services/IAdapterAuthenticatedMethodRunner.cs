namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public interface IAdapterAuthenticatedMethodRunner<out TAdapter> where TAdapter : IServerAdapter
    {
        Task<Result<TResult>> ExecuteForUser<TResult>(Func<TAdapter, Task<Result<TResult>>> func);
        Task<Result> ExecuteForUser(Func<TAdapter, Task<Result>> func);
    }
}
