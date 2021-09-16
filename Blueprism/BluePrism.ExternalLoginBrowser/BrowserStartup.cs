using BluePrism.ExternalLoginBrowser.Form;
using IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;
using NLog;

namespace BluePrism.ExternalLoginBrowser
{
    public class BrowserStartup
    {
        private readonly string _authenticationGatewayUrl;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string OidcClientIdentityProviderDiscoveryError = "Error loading discovery document";

        public BrowserStartup(string authenticationGatewayUrl)
        {
            _authenticationGatewayUrl = authenticationGatewayUrl;
        }
        
        public async Task<LoginResult> Login(LoginRequest loginRequest)
        {
            try
            {
                var browserFormFactory = new BrowserFormFactory();
                var chromiumBrowserFactory = new ChromiumLoginBrowserFactory();
                var browser = new Browser(browserFormFactory, chromiumBrowserFactory);

                var options = new OidcClientOptions()
                {
                    Authority = _authenticationGatewayUrl,
                    ClientId = "automate",
                    Scope = "openid bpserver",
                    RedirectUri = "http://localhost/automate",
                    Browser = browser,
                    Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                    ResponseMode = OidcClientOptions.AuthorizeResponseMode.FormPost
                };

                var client = new OidcClient(options);                
                return await client.LoginAsync(loginRequest);
            }
            catch (InvalidOperationException e)
            {
                Log.Error("OidcClient failed to connect: {0}", e.Message);

                if (e.Message.StartsWith(OidcClientIdentityProviderDiscoveryError))
                    return new LoginResult(IdentityModel.OidcClient.Browser.BrowserResultType.UnknownError.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OidcClient exception when connecting");
            }

            return new LoginResult(IdentityModel.OidcClient.Browser.BrowserResultType.UnknownError.ToString());
        }

        public async Task<LoginResult> LoginWithDisplayVisible()
        {
            return await Login(new LoginRequest
            {
                BrowserDisplayMode = IdentityModel.OidcClient.Browser.DisplayMode.Visible
            });
        }
    }
}
