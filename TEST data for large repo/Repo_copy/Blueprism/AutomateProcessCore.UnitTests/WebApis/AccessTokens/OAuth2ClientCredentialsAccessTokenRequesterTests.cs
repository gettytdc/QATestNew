#if UNITTESTS
using System.IO;
using System.Net;
using System.Text;
using BluePrism.AutomateProcessCore;
using BluePrism.Common.Security;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.AccessTokens
{
    public class OAuth2ClientCredentialsAccessTokenRequesterTests
    {
        private string _clientId;
        private SafeString _clientSecret;

        [SetUp]
        public void SetUp()
        {
            _clientId = "ben";
            _clientSecret = "password".AsSecureString();
        }

        [Test]
        public void BuildAccessTokenRequest_EveryRequest_ShouldHaveCorrectContentType()
        {
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.Method);
            requestMock.SetupProperty(x => x.ContentType);
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, string.Empty, _clientId, _clientSecret);
            Assert.That(requestMock.Object.ContentType, Is.EqualTo("application/x-www-form-urlencoded"));
        }

        [Test]
        public void BuildAccessTokenRequest_EveryRequest_ShouldBePost()
        {
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.Method);
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, "api", "ben", "password1".AsSecureString());
            Assert.That(requestMock.Object.Method, Is.EqualTo("POST"));
        }

        [Test]
        public void GetRequestContent_ScopeIsNull_ReturnGrantTypeOnly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var content = requester.GetRequestContent(null);
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("grant_type=client_credentials"));
        }

        [Test]
        public void GetRequestContent_ScopeIsEmpty_ReturnGrantTypeOnly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var content = requester.GetRequestContent(string.Empty);
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("grant_type=client_credentials"));
        }

        [Test]
        public void GetRequestContent_ScopeIsWhiteSpace_ReturnGrantTypeOnly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var content = requester.GetRequestContent(" ");
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("grant_type=client_credentials"));
        }

        [Test]
        public void GetRequestContent_ScopeHasValue_ReturnGrantTypeAndScope()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var content = requester.GetRequestContent("api");
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo($"grant_type=client_credentials&scope=api"));
        }

        [Test]
        public void GetRequestContent_ScopeHasEncodableCharacters_ReturnCorrectlyEncodedScope()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var content = requester.GetRequestContent("api/somebitofscope");
            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo($"grant_type=client_credentials&scope=api%2fsomebitofscope"));
        }

        [Test]
        public void BuildAccessTokenRequest_WithScope_ShouldSetContentLengthCorrectly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.ContentLength);
            requestMock.SetupProperty(x => x.ContentType);
            requestMock.SetupProperty(x => x.Method);
            var content = $"grant_type=client_credentials&scope=api";
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, "api", _clientId, _clientSecret);
            Assert.That(requestMock.Object.ContentLength, Is.EqualTo(content.Length));
        }

        [Test]
        public void BuildAccessTokenRequest_ScopeIsEmpty_ShouldSetContentLengthCorrectly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.ContentLength);
            requestMock.SetupProperty(x => x.ContentType);
            requestMock.SetupProperty(x => x.Method);
            var content = $"grant_type=client_credentials";
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, string.Empty, _clientId, _clientSecret);
            Assert.That(requestMock.Object.ContentLength, Is.EqualTo(content.Length));
        }

        [Test]
        public void BuildAccessTokenRequest_ScopeHasEncodableCharacters_ShouldSetContentLengthCorrectly()
        {
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetRequestStream()).Returns(new MemoryStream());
            requestMock.SetupProperty(x => x.ContentLength);
            requestMock.SetupProperty(x => x.ContentType);
            requestMock.SetupProperty(x => x.Method);
            var content = $"grant_type=client_credentials&scope=api%2fsomebitofscope";
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, "api/somebitofscope", _clientId, _clientSecret);
            Assert.That(requestMock.Object.ContentLength, Is.EqualTo(content.Length));
        }

        [Test]
        public void BuildAccessTokenRequest_RequestStreamThrows_ShouldThrowCorrectException()
        {
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Throws<WebException>();
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            Assert.Throws<AccessTokenException>(() =>
            {
                var argRequest = requestMock.Object;
                requester.BuildAccessTokenRequest(ref argRequest, string.Empty, _clientId, _clientSecret);
            });
        }

        [Test]
        public void GetOAuth2AccessToken_Throws_ShouldThrowCorrectException()
        {
            var streamMock = new Mock<Stream>();
            var requestMock = new Mock<HttpWebRequest>();
            requestMock.Setup(x => x.GetRequestStream()).Returns(streamMock.Object);
            requestMock.Setup(x => x.Headers).Returns(new WebHeaderCollection());
            requestMock.Setup(x => x.GetResponse()).Throws<WebException>();
            var requester = new OAuth2ClientCredentialsAccessTokenRequester();
            var argRequest = requestMock.Object;
            requester.BuildAccessTokenRequest(ref argRequest, string.Empty, _clientId, _clientSecret);
            Assert.Throws<AccessTokenException>(() => requester.RequestOAuth2AccessToken(requestMock.Object));
        }
    }
}
#endif
