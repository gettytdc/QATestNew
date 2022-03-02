using System.Net.Http;
using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer;

namespace BluePrism.StartUp.Modules
{
    public class AuthenticationServerUserMappingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<HttpClientWrapper>().As<IHttpClientWrapper>();
            builder.RegisterType<AccessTokenRequester>().As<IAccessTokenRequester>();
            builder.RegisterType<AuthenticationServerHttpRequester>().As<IAuthenticationServerHttpRequester>();
            builder.RegisterType<UniqueUsernameGenerator>().As<IUniqueUsernameGenerator>();
        }
    }
}
