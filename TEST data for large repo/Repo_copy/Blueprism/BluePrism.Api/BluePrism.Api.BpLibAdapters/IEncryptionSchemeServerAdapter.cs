namespace BluePrism.Api.BpLibAdapters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Domain;
    using Func;

    public interface IEncryptionSchemeServerAdapter : IServerAdapter
    {
        Task<Result<IEnumerable<EncryptionScheme>>> EncryptionSchemesGetSchemes();
    }
}
