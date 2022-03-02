namespace BluePrism.Api.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IEncryptionSchemeService
    {
        Task<Result<IEnumerable<EncryptionScheme>>> GetEncryptionSchemes();
    }
}
