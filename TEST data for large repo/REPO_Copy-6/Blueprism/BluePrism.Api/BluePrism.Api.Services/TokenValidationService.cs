namespace BluePrism.Api.Services
{
    using System.Threading.Tasks;
    using Func;

    public class TokenValidationService : ITokenValidationService
    {
        public Task<Result> ValidateToken(string token) =>
            ResultHelper.Succeed().ToTask();
    }
}
