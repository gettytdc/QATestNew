namespace BluePrism.Api.BpLibAdapters
{
    using System.Threading.Tasks;
    using Func;

    public interface IAuthServerAdapter : IServerAdapter
    {
        Task<Result<string>> GetAuthenticationServerUrl();
    }
}
