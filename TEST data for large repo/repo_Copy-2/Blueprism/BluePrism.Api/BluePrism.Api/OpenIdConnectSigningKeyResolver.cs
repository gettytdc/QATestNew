namespace BluePrism.Api
{
    using System.Linq;
    using Func;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    public class OpenIdConnectSigningKeyResolver : ISigningKeyResolver
    {
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private OpenIdConnectConfiguration _openIdConfig;

        public OpenIdConnectSigningKeyResolver(string authority) => _configurationManager =
            new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority.TrimEnd('/')}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

        private OpenIdConnectConfiguration RetrieveConfiguration() => _openIdConfig = _configurationManager.GetConfigurationAsync().Result;

        public SecurityKey[] GetSigningKeys(string kid) =>
            (_openIdConfig ?? RetrieveConfiguration())
                .Map(config => config.JsonWebKeySet.GetSigningKeys().Where(k => k.KeyId == kid)).ToArray();
    }
}
