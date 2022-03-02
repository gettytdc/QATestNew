namespace BluePrism.Api.BpLibAdapters
{
    using AutomateAppCore;
    using AutomateAppCore.Auth;

    public interface IUserStaticMethodWrapper
    {
        LoginResult LoginWithAccessToken(string machineName, string token, string locale, IServer server = null);
    }
}
