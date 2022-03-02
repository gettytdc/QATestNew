using System.IdentityModel.Tokens.Jwt;
using Autofac;
using BluePrism.AutomateAppCore.Auth;
using Microsoft.IdentityModel.Tokens;

namespace BluePrism.StartUp.Modules
{
    public class CredentialsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AccessTokenValidator>().As<IAccessTokenValidator>();
            builder.RegisterType<JwtSecurityTokenHandler>().As<ISecurityTokenValidator>();
            builder.RegisterType<DiscoveryDocumentRetriever>().As<IDiscoveryDocumentRetriever>();
            builder.RegisterType<AccessTokenClaimsParser>().As<IAccessTokenClaimsParser>().SingleInstance();
        }
    }
}
