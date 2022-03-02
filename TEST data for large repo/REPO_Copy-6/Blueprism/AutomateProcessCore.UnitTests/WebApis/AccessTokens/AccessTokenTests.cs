#if UNITTESTS
using System;
using BluePrism.AutomateProcessCore.WebApis.AccessTokens;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.AccessTokens
{
    public class AccessTokenTests
    {
        [Test]
        public void DeserializeAccessToken_WithExpiresIn_CreatesAccessTokenCorrectly()
        {
            var jsonResponse = "{\"access_token\":\"0E2ec88d738bc036dc4f5f8b9bba4502\",\"expires_in\":3600,\"token_type\":\"Bearer\"}";
            var dt = DateTime.UtcNow + TimeSpan.FromSeconds(3600d);
            var token = JsonConvert.DeserializeObject<AccessToken>(jsonResponse);
            Assert.That(Conversions.ToDate(token.ExpiryDate).Date, Is.EqualTo(dt.Date));
            Assert.That(Conversions.ToDate(token.ExpiryDate).Hour, Is.EqualTo(dt.Hour));
            Assert.That(token.Valid, Is.EqualTo(true));
            Assert.That(token.AccessToken, Is.EqualTo("0E2ec88d738bc036dc4f5f8b9bba4502"));
        }

        [Test]
        public void DeserializeAccessToken_NoExpiresIn_CreatesAccessTokenWithNoExpiryDate()
        {
            var jsonResponse = "{\"access_token\":\"0E2ec88d738bc036dc4f5f8b9bba4502\",\"token_type\":\"Bearer\"}";
            var token = JsonConvert.DeserializeObject<AccessToken>(jsonResponse);
            Assert.That(token.ExpiryDate, Is.EqualTo(null));
            Assert.That(token.Valid, Is.EqualTo(true));
            Assert.That(token.AccessToken, Is.EqualTo("0E2ec88d738bc036dc4f5f8b9bba4502"));
        }

        [Test]
        public void DeserializeAccessToken_ZeroExpiresIn_CreatesAccessTokenWithNoExpiryDate()
        {
            var jsonResponse = "{\"access_token\":\"0E2ec88d738bc036dc4f5f8b9bba4502\",\"expires_in\":0,\"token_type\":\"Bearer\"}";
            var token = JsonConvert.DeserializeObject<AccessToken>(jsonResponse);
            Assert.That(token.ExpiryDate, Is.EqualTo(null));
            Assert.That(token.Valid, Is.EqualTo(true));
            Assert.That(token.AccessToken, Is.EqualTo("0E2ec88d738bc036dc4f5f8b9bba4502"));
        }

        [Test]
        public void DeserializeAccessToken_EmptyResponse_CreatesEmptyToken()
        {
            var jsonResponse = "{}";
            var token = JsonConvert.DeserializeObject<AccessToken>(jsonResponse);
            Assert.That(token.ExpiryDate, Is.EqualTo(null));
            Assert.That(token.Valid, Is.EqualTo(true));
            Assert.That(token.AccessToken, Is.EqualTo(null));
        }

        [Test]
        public void GetExpiryDate_WithExpiry_ShouldReturnCorrectDate() => Assert.That(AccessToken.GetExpiryDate(60, new DateTime(2018, 12, 12, 1, 30, 30)), Is.EqualTo(new DateTime(2018, 12, 12, 1, 31, 30)));

        [Test]
        public void GetExpiryDate_WithNoExpiry_ShouldReturnNothing() => Assert.That(AccessToken.GetExpiryDate(default(int?), new DateTime(2018, 12, 12, 1, 30, 30)), Is.EqualTo(null));

        [Test]
        public void GetExpiryDate_WithZeroExpiry_ShouldReturnNothing() => Assert.That(AccessToken.GetExpiryDate(default(int?), new DateTime(2018, 12, 12, 1, 30, 30)), Is.EqualTo(null));

        [Test]
        public void GetExpiryDate_WithNegativeExpiry_ShouldReturnNothing() => Assert.That(AccessToken.GetExpiryDate(-25, new DateTime(2018, 12, 12, 1, 30, 30)), Is.EqualTo(null));
    }
}
#endif
