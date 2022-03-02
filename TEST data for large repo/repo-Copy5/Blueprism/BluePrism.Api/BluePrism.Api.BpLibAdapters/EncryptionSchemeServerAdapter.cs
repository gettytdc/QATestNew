namespace BluePrism.Api.BpLibAdapters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutomateAppCore;
    using Domain;
    using Func;
    using Mappers;

    using static ServerResultTask;

    public class EncryptionSchemeServerAdapter : IEncryptionSchemeServerAdapter
    {
        private readonly IServer _server;

        public EncryptionSchemeServerAdapter(IServer server) =>
            _server = server;

        public Task<Result<IEnumerable<EncryptionScheme>>> EncryptionSchemesGetSchemes() =>
            RunOnServer(() => _server
                .GetEncryptionSchemes()
                .Select(x => x.ToDomain()));
    }
}
