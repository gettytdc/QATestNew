namespace BluePrism.DocumentProcessing.Integration.UnitTests
{
    using System;
    using System.Text;
    using Core.Conversion;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;
    using Utilities.Functional;

    [TestFixture]
    public class JwtProviderTests : UnitTestBase<JwtProvider>
    {
        [Test]
        public void GetToken_ReturnsTokenWithCorrectUsername()
        {
            const string username = "TestUser";

            var token =
                ClassUnderTest.GetToken(username, new string[0], "Test", DateTime.UtcNow.AddDays(1))
                    .Map(ParseToken);

            string tokenUsername = token.Body.sub;
            tokenUsername.Should().Be(username);
        }

        [Test]
        public void GetToken_ContainsValidHeader()
        {
            var token =
                ClassUnderTest.GetToken("Test", new string[0], "Test", DateTime.UtcNow.AddDays(1))
                    .Map(ParseToken);

            string algorithm = token.Header.alg;
            string type = token.Header.typ;

            algorithm.Should().Be("HS256");
            type.Should().Be("JWT");
        }

        private static (dynamic Header, dynamic Body, string Signature) ParseToken(string token) =>
            token
                .Split('.')
                .Map(x => (
                    Header: x[0].Map(ParseBase64Json),
                    Body: x[1].Map(ParseBase64Json),
                    Signature: x[2]));

        private static dynamic ParseBase64Json(string base64Json) =>
            base64Json
                .Map(UrlBase64Convertor.FromBase64String)
                .Map(Encoding.UTF8.GetString)
                .Map(JObject.Parse);
    }
}