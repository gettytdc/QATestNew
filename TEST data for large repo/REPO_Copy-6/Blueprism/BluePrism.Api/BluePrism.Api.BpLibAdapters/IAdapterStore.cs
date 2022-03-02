namespace BluePrism.Api.BpLibAdapters
{
    using System.Threading.Tasks;
    using Func;

    public interface IAdapterStore<TAdapter> where TAdapter : IServerAdapter
    {
        Task<Result<TAdapter>> GetAdapterForToken(string token);

        Task<Result<TAdapter>> GetAnonymousAdapter();
    }
}
