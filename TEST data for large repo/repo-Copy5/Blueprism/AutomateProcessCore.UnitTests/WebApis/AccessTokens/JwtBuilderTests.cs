#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Security.Claims;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.UnitTests.WebApis;
using BluePrism.AutomateProcessCore.WebApis.AccessTokens;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using BluePrism.Common.Security;
using BluePrism.Core.Utility;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.AccessTokens
{
    public class JwtBuilderTests
    {
        private readonly DateTime _baseDate = new DateTime(1970, 1, 1, 0, 0, 0);
        private const string PrivateKey = @"-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDRdIxP5fUMyqD6\nX7+K8ZkkkYa2jpcb5TDIngMzVZxWmMXeAxG4/tVOUn/F7OpnjlEAQRYSul2h+0iI\nCiFsqESFMEI/t8tOXj+nSHVUQBUWqjBrpugRyZb9cd0xB/6lb6eylXzhHf0zEbAM\n4cNsiulmXNBDjacffTWp/Qco4FSoarce0mxX9UPtBsdCEXsNyFprRD6kDUHq//fq\nngmCJjiws8gXoR/mI8uYCp9vJ3/VV8Y5oxgjLMlLlPULKLaUPOlqIObeep4GFUci\nLlLHYEzKYal8o2fbrYGRE1JvApr0VBIIaweQozSKEW9LlJbZ9pxcYJbrylVt9+Pa\nZcUjZafVAgMBAAECggEAAML/aPb5dNTJJb/yuWBMN0bUNOvAfu3Ox0apKlGgYkGR\n+U8M4chYzD0ekStdutyt92Xv3PI9OZk+rUf5e89Xgx3ReoaCQG8KpOLC583dMG+U\nAirhjdcBgTsdxno0od4Nv7zYGcAl1elgONvFUyq6KJDOgmsMdYqGBxqQIRpCaeO1\nGtKkGIbews6MoAzuUklFUGdNKmEcwI2nkubengQ4YO3A3qVY9GFJYldoSGPbRDRb\nmWWCkQ54M+XKkQvT6PwvZkxwwJ09qhCBwExw7NHBTD53eZzyxh0zEkEASTeRfQeR\ntB0m/pRvkgIe7L7eX+G1Qk+fs16FkXjVX7JaqyWwIQKBgQD7ZplMPgRPHNxc5Cc0\nKXn9I62PPZ/5hPe6A35eoBPj6yoc7P9EG6CHcsG7MHYw0CZxiYNm8okfqcz+Y3kD\ngBi/JEOIMJ9YoRTx976o4EL16DuoM8tfZMQKraWbfRrJo1aVbfwQm+FRgJ/w/Jst\nsyyOgskvxtbe38scLGadwwLWuQKBgQDVSYDSOdfxOL0bT7ZRwgOa2sx/DJ4xfCbl\nlbrbsvwQ8RO7NSPmG+Md4GgWOvZr9MSyZpudP0e5tR8vZnUHD74RaAzvRPUb5EfH\n8ruvtbC6D99qGurzoYwspQ0t8EWVPTxuQ5WET/Q1oCcXsahdPwwkBzBGNkGVWosc\nUvz4pomL/QKBgERK47vlYPBIy8fkJo4x8jSE26H8wJdcuprqVgrIe36/pEoFS3ap\n9pq03tHF4WVzNe5Dx9FQc1WJn+k/BmGgC5jo9ftqEe9WmsT8YuunOClq88HGiCma\nyvKHp2QswPkjiOLA5OdyIOwt/8TMKi9o6+Kuvo5e+oHC038MxLd4gS3pAoGBAK7T\ndBNdtIKWZnO3kBNN+5R4Gj1dw2F+iYeQhrzE5lagtPKzqTxZyX1YzxjBtfFhjcVJ\n0+49y3YOX4SD8BMctUghSNDrbhoxsSDU90EcpcKtdgzMCaAv3+1MURRBqVed/SXI\nogr1WpCGOOT0C7R7FGGHvuseV/2rXPGXmKHcKEylAoGAAhayyMTjws2p02JtwmPZ\nDhHaLey4qqgIofvtm4mtu5hgH7BW2yw39j/n0hyBKba1lZS6R5udpEeYz14Q2fZ0\nhOnaSjZ1MFTEB4Zi6qCuk5uPc27BiI/C9677xkGd9AwTHXLdfBI2Qi+45N8TFS88\nC1IWJpa4pm+kB6+/WP4JfpI=\n-----END PRIVATE KEY-----\n";
        private const string PrivateKeyTrimmed = "MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDRdIxP5fUMyqD6X7+K8ZkkkYa2jpcb5TDIngMzVZxWmMXeAxG4/tVOUn/F7OpnjlEAQRYSul2h+0iICiFsqESFMEI/t8tOXj+nSHVUQBUWqjBrpugRyZb9cd0xB/6lb6eylXzhHf0zEbAM4cNsiulmXNBDjacffTWp/Qco4FSoarce0mxX9UPtBsdCEXsNyFprRD6kDUHq//fqngmCJjiws8gXoR/mI8uYCp9vJ3/VV8Y5oxgjLMlLlPULKLaUPOlqIObeep4GFUciLlLHYEzKYal8o2fbrYGRE1JvApr0VBIIaweQozSKEW9LlJbZ9pxcYJbrylVt9+PaZcUjZafVAgMBAAECggEAAML/aPb5dNTJJb/yuWBMN0bUNOvAfu3Ox0apKlGgYkGR+U8M4chYzD0ekStdutyt92Xv3PI9OZk+rUf5e89Xgx3ReoaCQG8KpOLC583dMG+UAirhjdcBgTsdxno0od4Nv7zYGcAl1elgONvFUyq6KJDOgmsMdYqGBxqQIRpCaeO1GtKkGIbews6MoAzuUklFUGdNKmEcwI2nkubengQ4YO3A3qVY9GFJYldoSGPbRDRbmWWCkQ54M+XKkQvT6PwvZkxwwJ09qhCBwExw7NHBTD53eZzyxh0zEkEASTeRfQeRtB0m/pRvkgIe7L7eX+G1Qk+fs16FkXjVX7JaqyWwIQKBgQD7ZplMPgRPHNxc5Cc0KXn9I62PPZ/5hPe6A35eoBPj6yoc7P9EG6CHcsG7MHYw0CZxiYNm8okfqcz+Y3kDgBi/JEOIMJ9YoRTx976o4EL16DuoM8tfZMQKraWbfRrJo1aVbfwQm+FRgJ/w/JstsyyOgskvxtbe38scLGadwwLWuQKBgQDVSYDSOdfxOL0bT7ZRwgOa2sx/DJ4xfCbllbrbsvwQ8RO7NSPmG+Md4GgWOvZr9MSyZpudP0e5tR8vZnUHD74RaAzvRPUb5EfH8ruvtbC6D99qGurzoYwspQ0t8EWVPTxuQ5WET/Q1oCcXsahdPwwkBzBGNkGVWoscUvz4pomL/QKBgERK47vlYPBIy8fkJo4x8jSE26H8wJdcuprqVgrIe36/pEoFS3ap9pq03tHF4WVzNe5Dx9FQc1WJn+k/BmGgC5jo9ftqEe9WmsT8YuunOClq88HGiCmayvKHp2QswPkjiOLA5OdyIOwt/8TMKi9o6+Kuvo5e+oHC038MxLd4gS3pAoGBAK7TdBNdtIKWZnO3kBNN+5R4Gj1dw2F+iYeQhrzE5lagtPKzqTxZyX1YzxjBtfFhjcVJ0+49y3YOX4SD8BMctUghSNDrbhoxsSDU90EcpcKtdgzMCaAv3+1MURRBqVed/SXIogr1WpCGOOT0C7R7FGGHvuseV/2rXPGXmKHcKEylAoGAAhayyMTjws2p02JtwmPZDhHaLey4qqgIofvtm4mtu5hgH7BW2yw39j/n0hyBKba1lZS6R5udpEeYz14Q2fZ0hOnaSjZ1MFTEB4Zi6qCuk5uPc27BiI/C9677xkGd9AwTHXLdfBI2Qi+45N8TFS88C1IWJpa4pm+kB6+/WP4JfpI=";
        private readonly Dictionary<string, clsProcessValue> _emptyParams = new Dictionary<string, clsProcessValue>();

        [TestCase(59, "01/01/1970 00:00:59")]
        [TestCase(3600, "01/01/1970 01:00:00")]
        [TestCase(0, "01/01/1970 00:00:00")]
        public void GetExpiryDate_WithExpiry_ReturnsCorrectDate(int expiresIn, DateTime? expectedResult)
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(_baseDate);
            var builder = new JwtBuilder(clockMock.Object);
            var expiryDate = builder.GetExpiryDate(expiresIn);
            Assert.That(expiryDate, Is.EqualTo((object)expectedResult));
        }

        [Test]
        public void TrimPrivateKey_ValidKey_ReturnsCorrectValue()
        {
            var expectedResult = new SafeString(PrivateKeyTrimmed);
            var trimmedResult = JwtBuilder.TrimPrivateKey(new SafeString(PrivateKey)).AsString();
            Assert.That(trimmedResult, Is.EqualTo(expectedResult.AsString()));
        }

        [Test]
        public void TrimPrivateKey_MissingPrefix_ThrowsCorrectException()
        {
            var keyNoPrefix = new SafeString(PrivateKey.Replace("-----BEGIN PRIVATE KEY-----", ""));
            Assert.That(() => JwtBuilder.TrimPrivateKey(keyNoPrefix), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void TrimPrivateKey_MissingSuffix_ThrowsCorrectException()
        {
            var keyNoSuffix = new SafeString(PrivateKey.Replace("-----END PRIVATE KEY-----", ""));
            Assert.That(() => JwtBuilder.TrimPrivateKey(keyNoSuffix), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void TrimPrivateKey_EndsInBackslash_ThrowsCorrectException()
        {
            var key = new SafeString(@"\n23456\n9\");
            Assert.That(() => JwtBuilder.TrimPrivateKey(key), Throws.TypeOf<ArgumentException>(), $@"Unexpected escape character sequence \ found at position 10 in private key. Only \n is supported.");
        }

        [Test]
        public void TrimPrivateKey_ContainsOtherEscapeChars_ThrowsCorrectException()
        {
            var key = new SafeString(@"\n23\t6\n9\");
            Assert.That(() => JwtBuilder.TrimPrivateKey(key), Throws.TypeOf<ArgumentException>(), $@"Unexpected escape character sequence \t found at position 4 in private key. Only \n is supported.");
        }

        [Test]
        public void BuildJwt_FullConfiguration_DoesNotThrow()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var cred = new TestCredential("testCredential", "cloud-language@cognitive-services-187312.iam.gserviceaccount.com", PrivateKey);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", "https://www.googleapis.com/auth/cloud-language  https://www.googleapis.com/auth/cloud-platform", "dave.smith@example.com", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            Assert.DoesNotThrow(() => builder.BuildJwt(config, cred, _emptyParams));
        }

        [Test]
        public void BuildJwt_EmptySubject_DoesNotThrow()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var cred = new TestCredential("testCredential", "cloud-language@cognitive-services-187312.iam.gserviceaccount.com", PrivateKey);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", "https://www.googleapis.com/auth/cloud-language  https://www.googleapis.com/auth/cloud-platform", string.Empty, 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            Assert.DoesNotThrow(() => builder.BuildJwt(config, cred, _emptyParams));
        }

        [Test]
        public void BuildJwt_EmptyScope_DoesNotThrow()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var cred = new TestCredential("testCredential", "cloud-language@cognitive-services-187312.iam.gserviceaccount.com", PrivateKey);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", string.Empty, "dave.smith@example.com", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            Assert.DoesNotThrow(() => builder.BuildJwt(config, cred, _emptyParams));
        }

        [Test]
        public void GetClaims_WithScopeAndSubject_ReturnsAll()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            const string scopeValue = " https://www.googleapis.com/auth/cloud-language  https://www.googleapis.com/auth/cloud-platform";
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", scopeValue, "testSubject.com", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            var expectedResult = new List<Claim>() { new Claim("scope", scopeValue), new Claim("sub", "testSubject.com") };
            var result = builder.GetClaims(config, _emptyParams);
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void GetClaims_WithEmptyScopeAndSubject_ReturnsNone()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", string.Empty, string.Empty, 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            var expectedResult = new List<Claim>();
            var result = builder.GetClaims(config, _emptyParams);
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void GetClaims_WithWhiteSpaceScopeAndSubject_ReturnsNone()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", "    ", "    ", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            var expectedResult = new List<Claim>();
            var result = builder.GetClaims(config, _emptyParams);
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void GetClaims_WithParameterizedSubject_ReturnsCorrectly()
        {
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(new DateTime());
            var builder = new JwtBuilder(clockMock.Object);
            var config = new JwtConfiguration("https://www.googleapis.com/oauth2/v4/token", "testScope", "[testSubject]", 120, new AuthenticationCredential("testCredential", true, "testParamName"));
            var expectedResult = new List<Claim> { new Claim("scope", "testScope"), new Claim("sub", "test.mail@test.com") };
            var parameters = new Dictionary<string,clsProcessValue>() { { "testSubject", new clsProcessValue("test.mail@test.com") } };
            var result = builder.GetClaims(config, parameters);
            result.ShouldBeEquivalentTo(expectedResult);
        }
    }
}
#endif
