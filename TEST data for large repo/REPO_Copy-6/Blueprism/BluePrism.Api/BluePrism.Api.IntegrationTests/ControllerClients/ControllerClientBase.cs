namespace BluePrism.Api.IntegrationTests.ControllerClients
{
    using System;
    using System.Net.Http;
    using Func;
    using Newtonsoft.Json;
    using System.Text;
    using System.Web.Http;
    using Castle.Core.Internal;
    using CommonTestClasses;
    using Domain;

    using static GlobalRoutePrefixProvider;

    public abstract class ControllerClientBase
    {
        protected HttpClient Client { get; }
        protected string BaseRoute { get; }

        protected ControllerClientBase(HttpClient client, Type controllerType,  OpenIdConfiguration openIdConfiguration, bool defaultAuthEnable = true)
        {
            var controllerPrefix =
                controllerType.GetAttribute<RoutePrefixAttribute>()?.Prefix
                ?? throw new NonPrefixAttributeControllerException(
                    $"Controller {controllerType.Name} must have a {nameof(RoutePrefixAttribute)} attribute");

            BaseRoute = $"{GlobalRoutePrefix}/{controllerPrefix}";
            Client = client;
            if (defaultAuthEnable)
            {
                Client.SetBearerToken(AuthHelper.GenerateToken(openIdConfiguration.Authority, openIdConfiguration.Audience));
            }
        }

        protected HttpRequestMessage CreateHttpRequest(string endpoint, HttpMethod method, object data, string token = "") =>
            new HttpRequestMessage(method, endpoint)
                .Tee(r => r.Content =
                    data
                        .Map(JsonConvert.SerializeObject)
                        .Map(x => new StringContent(x, Encoding.UTF8, "application/json")))
                .Tee(r => TryAddAuthorizationHeader(r, token));

        private static HttpRequestMessage TryAddAuthorizationHeader(HttpRequestMessage message, string token)
            => string.IsNullOrWhiteSpace(token)
                ? message
                : message.Tee(x => x.Headers.Add("Authorization", $"Bearer {token}"));
    }
}
