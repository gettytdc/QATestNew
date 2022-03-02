namespace BluePrism.Api.Utilities.MockOpenIdConnectServer.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using IdentityModel.Client;

    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> GetValidToken()
        {
            // DO NOT COPY THIS CODE INTO ANYTHING USED IN PRODUCTION!!!
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            var result = await
                new TokenClient(Url.Content("/connect/token"), AuthConfiguration.ClientId, "testtesttest")
                    .RequestResourceOwnerPasswordAsync("test", "test", "bp-api");

            return Ok(result.AccessToken);
        }
    }
}
