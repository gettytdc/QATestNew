namespace BluePrism.Api.Utilities.MockOpenIdConnectServer
{
    using System.Threading.Tasks;
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.Default;

    public class CustomTokenService : DefaultTokenService
    {
        public CustomTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events)
            : base(options, claimsProvider, tokenHandles, signingService, events)
        {
        }

        public CustomTokenService(IdentityServerOptions options, IClaimsProvider claimsProvider, ITokenHandleStore tokenHandles, ITokenSigningService signingService, IEventService events, OwinEnvironmentService owinEnvironmentService)
            : base(options, claimsProvider, tokenHandles, signingService, events, owinEnvironmentService)
        {
        }

        public override async Task<Token> CreateAccessTokenAsync(TokenCreationRequest request)
        {
            var token = await base.CreateAccessTokenAsync(request);
            token.Audience = AuthConfiguration.Audience;
            
            return token;
        }
    }
}
