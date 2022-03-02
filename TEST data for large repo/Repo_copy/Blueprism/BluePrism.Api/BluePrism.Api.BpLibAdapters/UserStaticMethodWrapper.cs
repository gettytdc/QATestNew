namespace BluePrism.Api.BpLibAdapters
{
    using AutomateAppCore;
    using AutomateAppCore.Auth;

    public class UserStaticMethodWrapper : IUserStaticMethodWrapper
    {
        public LoginResult LoginWithAccessToken(string machineName, string token, string locale, IServer server) =>
            User.LoginWithAccessToken(machineName, token, locale, server);
    }
}
