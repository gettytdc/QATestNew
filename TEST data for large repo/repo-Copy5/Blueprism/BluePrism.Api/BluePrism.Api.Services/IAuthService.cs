namespace BluePrism.Api.Services
{
    using System.Threading.Tasks;
    using Func;

    public interface IAuthService
    {
        Task<Result<string>> GetAuthServerUrl();
    }
}
