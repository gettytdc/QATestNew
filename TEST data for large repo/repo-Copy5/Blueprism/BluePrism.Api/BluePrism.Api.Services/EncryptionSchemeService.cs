namespace BluePrism.Api.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain;
    using Func;    

    public class EncryptionSchemeService : IEncryptionSchemeService
    {
        private readonly IAdapterAuthenticatedMethodRunner<IEncryptionSchemeServerAdapter> _encryptionSchemeRunner;

        public EncryptionSchemeService(
            IAdapterAuthenticatedMethodRunner<IEncryptionSchemeServerAdapter> encryptionSchemeRunner) =>
            _encryptionSchemeRunner = encryptionSchemeRunner;
        
        public Task<Result<IEnumerable<EncryptionScheme>>> GetEncryptionSchemes() =>
            _encryptionSchemeRunner.ExecuteForUser(s => s.EncryptionSchemesGetSchemes());
    }
}
