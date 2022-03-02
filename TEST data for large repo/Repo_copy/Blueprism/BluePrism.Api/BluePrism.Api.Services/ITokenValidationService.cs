namespace BluePrism.Api.Services
{
    using System.Threading.Tasks;
    using Func;

    public interface ITokenValidationService
    {
        Task<Result> ValidateToken(string token);
    }
}
