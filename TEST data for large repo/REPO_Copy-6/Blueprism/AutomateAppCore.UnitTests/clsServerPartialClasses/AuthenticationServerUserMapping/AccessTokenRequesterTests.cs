using System;
using System.Security;
using System.Threading.Tasks;
using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer;
using BluePrism.Core.Utility;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using IdentityModel.Client;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.AuthenticationServerUserMapping
{
    [TestFixture]
    public class AccessTokenRequesterTests : UnitTestBase<AccessTokenRequester>
    {       

        private const string TestAccessToken = "test_access_token";
        private const int TestAccessTokenExpiresIn = 100;

        public override void Setup(Action<ContainerBuilder> container)
        {
            base.Setup(container);                  

            var now = DateTime.UtcNow;            
            GetMock<ISystemClock>().Setup(m => m.UtcNow).Returns(now);
                        
            GetMock<ITokenResponseWrapper>().Setup(m => m.AccessToken).Returns(TestAccessToken);
            GetMock<ITokenResponseWrapper>().Setup(m => m.ExpiresIn).Returns(TestAccessTokenExpiresIn);

            GetMock<IHttpClientWrapper>()
                .Setup(m => m.RequestClientCredentialsTokenAsync(It.IsAny<ClientCredentialsTokenRequest>()))
                .ReturnsAsync(GetMock<ITokenResponseWrapper>().Object);
        }

        [Test]
        public void Ctor_WithMissingHttpClientWrapper_ThrowsArgumentNullException()
        {
            Action ctorAction = () => new AccessTokenRequester(null, GetMock<ISystemClock>().Object);

            ctorAction.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WithMissingSystemClock_ThrowsArgumentNullException()
        {
            Action ctorAction = () => new AccessTokenRequester(GetMock<IHttpClientWrapper>().Object, null);

            ctorAction.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public async Task RequestAccessToken_WithValidRequest_ReturnsExpectedToken()
        {
            var result = await ClassUnderTest.RequestAccessToken(string.Empty, string.Empty, new SecureString());

            result.Should().NotBeNull();
            result.AccessToken.ShouldBeEquivalentTo(TestAccessToken);
            result.ExpiryDate.ShouldBeEquivalentTo(GetMock<ISystemClock>().Object.UtcNow.UtcDateTime.AddSeconds(TestAccessTokenExpiresIn));
        }

        [Test]
        public async Task RequestAccessToken_TokenRequestIsError_ReturnsNull()
        {
            GetMock<ITokenResponseWrapper>().Setup(m => m.IsError).Returns(true);

            var result = await ClassUnderTest.RequestAccessToken(string.Empty, string.Empty, new SecureString());

            result.Should().BeNull();
        }

        [Test]
        public async Task RequestAccessToken_TokenRequestThrowsException_ReturnsNull()
        {
            GetMock<IHttpClientWrapper>()
                .Setup(m => m.RequestClientCredentialsTokenAsync(It.IsAny<ClientCredentialsTokenRequest>()))
                .Throws(new Exception());

            var result = await ClassUnderTest.RequestAccessToken(string.Empty, string.Empty, new SecureString());

            result.Should().BeNull();
        }
    }
}
