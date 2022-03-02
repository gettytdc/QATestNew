using System;
using System.Security.Claims;
using BluePrism.AutomateAppCore.Auth;
using FluentAssertions;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Auth
{
    public class AccessTokenValidatorTests
    {
        private Mock<ISecurityTokenValidator> _tokenHandlerMock;
        private Mock<IDiscoveryDocumentRetriever> _discoveryDocumentRetrieverMock;

        [SetUp]
        public void Setup()
        {
            _tokenHandlerMock = new Mock<ISecurityTokenValidator>();
            _discoveryDocumentRetrieverMock = new Mock<IDiscoveryDocumentRetriever>();
            _discoveryDocumentRetrieverMock
                .Setup(x => x.GetDiscoveryDocument(It.IsAny<string>()))
                .Returns(new OpenIdConnectConfiguration());
        }

        [Test]
        public void Validate_ValidToken_ShouldReturnExpectedClaims()
        {
            var principal = new ClaimsPrincipal();
            var nameClaim = new Claim(ClaimTypes.NameIdentifier, "John McTest");
            principal.AddIdentity(new ClaimsIdentity(new Claim[] { nameClaim }));

            var token = It.IsAny<SecurityToken>();
            _tokenHandlerMock
                .Setup(h => h.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out token))
                .Returns(principal);

            var validator = new AccessTokenValidator(_tokenHandlerMock.Object, _discoveryDocumentRetrieverMock.Object);
            var claims = validator.Validate("12343", "http://identityserver:3000");

            claims.Should().Be(principal);
        }

        [Test]
        public void Validate_TokenInvalid_ShouldThrow()
        {
            var token = It.IsAny<SecurityToken>();
            _tokenHandlerMock
                .Setup(h => h.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(), out token))
                .Throws(new SecurityTokenValidationException("testMessage", new SecurityTokenInvalidIssuerException()));

            var validator = new AccessTokenValidator(_tokenHandlerMock.Object, _discoveryDocumentRetrieverMock.Object);

            Action validate = () => validator.Validate(It.IsAny<string>(), It.IsAny<string>());

            validate
                .ShouldThrow<SecurityTokenValidationException>()
                .WithMessage("testMessage")
                .WithInnerException<SecurityTokenInvalidIssuerException>();
        }
    }
}
