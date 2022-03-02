namespace BluePrism.Api.BpLibAdapters
{
    using AutomateAppCore;
    using Func;

    public interface IServerStore
    {
        Result<IServer> GetUnkeyedServerInstance();
        Result<IServer> GetServerInstanceForToken(string token);
        void CloseServerInstance(string token);
    }
}
