using BluePrism.AutomateAppCore;
using BluePrism.AutomateProcessCore.WebApis.Credentials;
using System.Collections.Generic;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// A service for retrieving credentials for use with data gateways processes.
    /// </summary>
    public class CredentialService : ICredentialService
    {

        private IServer _server;

        public CredentialService(IServer server)
        {
            _server = server;
        }

        public List<string> GetAllCredentialsInfo()
        {
            return _server.GetDataGatewayCredentials();
        }

        public ICredential GetCredential(string credentialName)
        {
            return _server.RequestCredentialForDataGatewayProcess(credentialName);
        }

    }
}
