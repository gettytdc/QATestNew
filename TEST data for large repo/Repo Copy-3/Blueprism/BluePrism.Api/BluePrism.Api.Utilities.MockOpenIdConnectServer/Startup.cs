using Microsoft.Owin;

[assembly: OwinStartup(typeof(BluePrism.Api.Utilities.MockOpenIdConnectServer.Startup))]

namespace BluePrism.Api.Utilities.MockOpenIdConnectServer
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Http;
    using Common.Security;
    using IdentityServer3.Core.Configuration;
    using IdentityServer3.Core.Models;
    using IdentityServer3.Core.Services;
    using IdentityServer3.Core.Services.InMemory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();

            var contractResolver =
                new CamelCasePropertyNamesContractResolver { NamingStrategy = { ProcessDictionaryKeys = false } };
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = contractResolver;
            httpConfiguration.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Culture = CultureInfo.InvariantCulture;

            httpConfiguration.Formatters.JsonFormatter.SerializerSettings.Converters
                .Add(new StringEnumConverter());

            httpConfiguration.MapHttpAttributeRoutes();

            var certificate = new X509Certificate2(
                ConfigurationManager.AppSettings["TestCert.Path"],
                ConfigurationManager.AppSettings["TestCert.Password"].AsSecureString(),
                X509KeyStorageFlags.UserKeySet);

            AuthConfiguration.ClientId = ConfigurationManager.AppSettings["Auth.ClientId"];
            AuthConfiguration.Audience = ConfigurationManager.AppSettings["Auth.Audience"];

            var serviceFactory = new IdentityServerServiceFactory()
                .UseInMemoryUsers(GetUsers())
                .UseInMemoryClients(GetClients())
                .UseInMemoryScopes(GetScopes());
            serviceFactory.TokenService = new Registration<ITokenService, CustomTokenService>();

            app.UseIdentityServer(new IdentityServerOptions
            {
                SiteName = "Mock OpenID Connect Server",
                SigningCertificate = certificate,
                Factory = serviceFactory,
            });

            app.UseWebApi(httpConfiguration);
        }

        private IEnumerable<Scope> GetScopes() =>
            new[]
            {
                new Scope
                {
                    Name = AuthConfiguration.Audience,
                    DisplayName = "Blue Prism API (Test)",
                },
                new Scope
                {
                    Name = "bpserver",
                    DisplayName = "Blue Prism Server",
                },
                new Scope
                {
                    Name = IdentityServer3.Core.Constants.StandardScopes.OfflineAccess,
                },
            };

        private IEnumerable<Client> GetClients() =>
            new[]
            {
                new Client
                {
                    ClientName = "API",
                    ClientId = AuthConfiguration.ClientId,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("testtesttest".Sha256())
                    },
                    Flow = Flows.ResourceOwner,
                    AllowedScopes = new List<string>
                    {
                        AuthConfiguration.Audience,
                        "bpserver",
                        IdentityServer3.Core.Constants.StandardScopes.OfflineAccess,
                    },
                    Enabled = true,
                },
            };

        private List<InMemoryUser> GetUsers() =>
            new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Enabled = true,
                    Username = "test",
                    Password = "test",
                    Subject = "2",
                    Claims = new []
                    {
                        new Claim("User Name", "Test"),
                    }
                },
            };
    }
}
