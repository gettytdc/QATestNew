using System;
using System.Security.Claims;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Auth
{
    [TestFixture]
    public class AccessTokenParserTests : UnitTestBase<AccessTokenClaimsParser>
    {
        [Test]
        public void GetUserId_NoNameClaim_ShouldThrowException()
        {
            var claims = new ClaimsPrincipal();
            Action getUserId = () => ClassUnderTest.GetUserId(claims);
            getUserId.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetUserId_NameClaimValueNotAGuid_ShouldThrowException()
        {
            var claims = new ClaimsPrincipal();
            var nameClaim = new Claim(ClaimTypes.NameIdentifier, "IAmNotAGuid");
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { nameClaim }));

            Action getUserId = () => ClassUnderTest.GetUserId(claims);

            getUserId.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetUserId_NameValueIsValidGuid_ShouldReturnUserId()
        {
            var claims = new ClaimsPrincipal();
            var userId = Guid.NewGuid().ToString();
            var nameClaim = new Claim(ClaimTypes.NameIdentifier, userId);
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { nameClaim }));

            var result = ClassUnderTest.GetUserId(claims);

            result.Should().Be(userId);
        }

        [Test]
        public void GetAuthenticationTime_NoAuthTimeClaim_ShouldReturnNull()
        {
            var claims = new ClaimsPrincipal();
            var result = ClassUnderTest.GetAuthenticationTime(claims);            

            result.Should().Be(null);
        }

        [Test]
        public void GetAuthenticationTime_AuthTimeClaimIsNotALong_ShouldThrowException()
        {
            var claims = new ClaimsPrincipal();
            var authTimeClaim = new Claim("auth_time", "IAmNotALong");
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { authTimeClaim }));

            Action getAuthenticationTime = () => ClassUnderTest.GetAuthenticationTime(claims);

            getAuthenticationTime.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetAuthenticationTime_AuthTimeClaimHasValidValue_ShouldReturnExpectedDate()
        {
            var claims = new ClaimsPrincipal();
            var now = DateTimeOffset.UtcNow;
            var epochSeconds = now.ToUnixTimeSeconds();
            var authTimeClaim = new Claim("auth_time", epochSeconds.ToString());
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { authTimeClaim }));

            var result = ClassUnderTest.GetAuthenticationTime(claims).Value;

            result.ToUnixTimeSeconds().Should().Be(epochSeconds);
        }

        [Test]
        public void GetIssuer_NoIssuerClaim_ShouldReturnExpectedDate()
        {
            var claims = new ClaimsPrincipal();
            Action getIssuer = () => ClassUnderTest.GetIssuer(claims);
            getIssuer.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetIssuer_IssuerClaimHasValue_ShouldReturnIssuer()
        {
            var claims = new ClaimsPrincipal();
            var issuer = "https://supersecureserver.com";
            var issuerClaim = new Claim("iss", issuer);
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { issuerClaim }));

            var result = ClassUnderTest.GetIssuer(claims);

            result.Should().Be(issuer);
        }

        [Test]
        public void GetId_NoIdClaim_ShouldReturnNUll()
        {
            var claims = new ClaimsPrincipal();
            var result = ClassUnderTest.GetId(claims);

            result.Should().Be(null);
        }

        [Test]
        public void GetId_IdClaimValueNotAGuid_ShouldThrowException()
        {
            var claims = new ClaimsPrincipal();
            var idClaim = new Claim("id", "IAmNotAGuid");
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { idClaim }));

            Action getId = () => ClassUnderTest.GetId(claims);

            getId.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetId_IdValueIsValidGuid_ShouldReturnId()
        {
            var claims = new ClaimsPrincipal();
            var userId = Guid.NewGuid().ToString();
            var idClaim = new Claim("id", userId);
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { idClaim }));

            var result = ClassUnderTest.GetId(claims);

            result.Should().Be(userId);
        }

        [Test]
        public void GetClientId_NoClientIdClaim_ShouldThrowException()
        {
            var claims = new ClaimsPrincipal();
            Action getClientId = () => ClassUnderTest.GetClientId(claims);
            getClientId.ShouldThrow<SecurityTokenValidationException>();
        }

        [Test]
        public void GetClientId_ClientIdClaimHasValue_ShouldReturnCllientId()
        {
            var claims = new ClaimsPrincipal();
            var clientId = "client_id_value";
            var clientIdClaim = new Claim("client_id", clientId);
            claims.AddIdentity(new ClaimsIdentity(new Claim[] { clientIdClaim }));

            var result = ClassUnderTest.GetClientId(claims);

            result.Should().Be(clientId);
        }
    }
}
