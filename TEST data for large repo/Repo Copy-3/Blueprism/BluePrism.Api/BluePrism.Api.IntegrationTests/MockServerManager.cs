namespace BluePrism.Api.IntegrationTests
{
    using System.Collections.Generic;
    using AutomateAppCore;
    using AutomateAppCore.Auth;

    public class MockServerManager : ServerManager
    {
        public MockServerManager(IServer server)
        {
            mServer = server;
        }

        public override void OpenConnection(clsDBConnectionSetting cons, Dictionary<string, clsEncryptionScheme> keys, ref IUser systemUser)
        {
        }

        public override void CloseConnection()
        {
        }
    }
}
