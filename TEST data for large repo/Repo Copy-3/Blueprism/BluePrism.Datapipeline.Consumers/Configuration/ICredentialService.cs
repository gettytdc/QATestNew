using BluePrism.AutomateProcessCore.WebApis.Credentials;
using System.Collections.Generic;


namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// A service for retrieving credentials for use with data gateways processes.
    /// </summary>
    public interface ICredentialService
    {
        List<string> GetAllCredentialsInfo();

        ICredential GetCredential(string credentialId);
    }
}
