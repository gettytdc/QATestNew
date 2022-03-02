namespace BluePrism.Api.Services
{
    using System;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;

    public interface IAdapterAnonymousMethodRunner<out TAdapter> where TAdapter : IServerAdapter
    {
        Task<Result<TResult>> Execute<TResult>(Func<TAdapter, Task<Result<TResult>>> func);
    }
}
