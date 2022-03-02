#if UNITTESTS
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.UnitTests.WebApis;
using BluePrism.AutomateProcessCore.WebApis.AccessTokens;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using BluePrism.AutomateProcessCore.WebApis.Credentials;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.AccessTokens
{
    public class OAuth2JwtBearerTokenAccessTokenRequesterTests
    {
        private readonly Dictionary<string, clsProcessValue> _params = new Dictionary<string, clsProcessValue>();
        private TestCredential _credential;
        private JwtConfiguration _jwtConfig;
        private const string JwtGrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        [SetUp]
        public void SetUp()
        {
            _credential = new TestCredential("testCredential", "cloud-language@cognitive-services-187312.iam.gserviceaccount.com", "testPrivateKey");
            _jwtConfig = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", "https://www.googleapis.com/auth/cloud-language  https://www.googleapis.com/auth/cloud-platform", "dave.smith@example.com", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
        }

        [Test]
        public void BuildAccessTokenRequest_EveryRequest_ShouldHaveCorrectContentType()
        {
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("testjwt");
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.Method);
            requestMock.SetupProperty(x => x.ContentType);
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            requester.BuildAccessTokenRequest(requestMock.Object, jwt);
            Assert.That(requestMock.Object.ContentType, Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public void BuildAccessTokenRequest_EveryRequest_ShouldBePost()
        {
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("testjwt");
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.Method);
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            requester.BuildAccessTokenRequest(requestMock.Object, jwt);
            Assert.That(requestMock.Object.Method, Is.EqualTo("POST"));
        }

        [Test]
        public void BuildAccessTokenRequest_RequestStreamThrows_ShouldThrowCorrectException()
        {
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("testjwt");
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Throws<WebException>();
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            Assert.Throws<AccessTokenException>(() => requester.BuildAccessTokenRequest(requestMock.Object, jwt));
        }

        [Test]
        public void GetRequestContent_CorrectlyReturnsGrantTypeAndJwt()
        {
            var jwtBearerGrantType = HttpUtility.UrlEncode(JwtGrantType);
            var requestParameters = $"grant_type={jwtBearerGrantType}&assertion={"testjwt"}";
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("testjwt");
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            var content = requester.GetRequestContent(jwt);
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo(requestParameters));
        }

        [Test]
        public void GetRequestContent_ScopeHasEncodableCharacters_ReturnsCorrectlyEncodedScope()
        {
            var jwtBearerGrantType = HttpUtility.UrlEncode(JwtGrantType);
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("test+jwt/test123");
            var requestParameters = $"grant_type={jwtBearerGrantType}&assertion={"test%2bjwt%2ftest123"}";
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            var content = requester.GetRequestContent(jwt);
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo(requestParameters));
        }

        [Test]
        public void GetOAuth2WithJwtAccessToken_Throws_ShouldThrowCorrectException()
        {
            var streamMock = new Mock<Stream>();
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Returns(streamMock.Object);
            requestMock.Setup(x => x.GetResponse()).Throws<WebException>();
            var builderMock = new Mock<IJwtBuilder>();
            builderMock.Setup(x => x.BuildJwt(_jwtConfig, _credential, _params)).Returns("testjwt");
            var jwt = builderMock.Object.BuildJwt(_jwtConfig, _credential, _params);
            var requester = new OAuth2JwtBearerTokenAccessTokenRequester(builderMock.Object);
            requester.BuildAccessTokenRequest(requestMock.Object, jwt);
            Assert.Throws<AccessTokenException>(() => requester.RequestOAuth2WithJwtAccessToken(requestMock.Object));
        }
    }
}
#endif
