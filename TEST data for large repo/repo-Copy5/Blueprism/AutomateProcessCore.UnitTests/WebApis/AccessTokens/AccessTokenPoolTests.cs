#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Net.Http;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateProcessCore.UnitTests.WebApis;
using BluePrism.AutomateProcessCore.WebApis;
using BluePrism.AutomateProcessCore.WebApis.AccessTokens;
using BluePrism.AutomateProcessCore.WebApis.Authentication;
using BluePrism.AutomateProcessCore.WebApis.Credentials;
using BluePrism.AutomateProcessCore.WebApis.RequestHandling;
using BluePrism.Core.Utility;
using Moq;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.WebApis.AccessTokens
{
    public class AccessTokenPoolTests
    {
        private AccessTokenPool _classUnderTest;
        private Guid _webApiId;
        private ActionContext _context;
        private AccessTokenPoolKey _accessTokenPoolKey;
        private DateTime _utcNow;
        private Mock<IAccessTokenRequester> _requesterMock;
        private readonly clsSession _session = new clsSession(new Guid("8fa4065c-2dad-4f5e-ac56-d5bbb61b6722"), 105, new WebConnectionSettings(5, 5, 5, new List<UriWebConnectionSettings>()));

        [SetUp]
        public void SetUp()
        {
            var authenticationMock = new Mock<IAuthentication>();
            var configuration = new WebApiConfigurationBuilder().WithCommonAuthentication(authenticationMock.Object).WithAction("Action1", HttpMethod.Get, "/api/action1").Build();
            _webApiId = Guid.NewGuid();
            _context = new ActionContext(_webApiId, configuration, "Action1", new Dictionary<string, clsProcessValue>(), _session);
            _accessTokenPoolKey = new AccessTokenPoolKey(_webApiId, TestCredential.Frank.Name);
            _utcNow = DateTime.UtcNow;
            _requesterMock = new Mock<IAccessTokenRequester>();
            var clockMock = new Mock<ISystemClock>();
            clockMock.Setup(x => x.UtcNow).Returns(_utcNow);
            _classUnderTest = new AccessTokenPool(clockMock.Object);
        }

        [Test]
        public void GetAccessToken_NoToken_ShouldGetNewToken()
        {
            var accessToken = new AccessToken("accesstoken", default(DateTime?), true);
            SetUpAccessTokens(accessToken, null);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken));
        }

        private AccessToken GetToken(ICredential credential) => _classUnderTest.GetAccessToken(_context, credential, _requesterMock.Object);

        [Test]
        public void GetAccessToken_ValidAccessTokenWithNoExpiry_ShouldReturnExistingToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, null);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken1));
        }

        [Test]
        public void GetAccessToken_ValidAndNotExpiredAccessToken_ShouldReturnExistingToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            SetUpAccessTokens(accessToken1, null);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken1));
        }

        [Test]
        public void GetAccessToken_ValidAndExpiredAccessToken_ShouldReturnNewToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(-1), true);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken2));
        }

        [Test]
        public void GetAccessToken_InvalidAccessTokenWithNoExpiry_ShouldReturnNewToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", default(DateTime?), false);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken2));
        }

        [Test]
        public void GetAccessToken_InvalidAndNotExpiredAccessToken_ShouldReturnNewToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), false);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken2));
        }

        [Test]
        public void GetAccessToken_InvalidAndExpiredAccessToken_ShouldReturnNewToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(-1), false);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            GetToken(TestCredential.Frank);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken2));
        }

        [Test]
        public void GetAccessToken_SameWebApiDifferentCredential_ShouldReturnNewToken()
        {
            var accessToken1 = new AccessToken("accesstoken1", default(DateTime?), true);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            GetToken(TestCredential.Barry);
            Assert.That(GetToken(TestCredential.Frank), Is.EqualTo(accessToken2));
        }

        [Test]
        public void CanAccessTokenBeUsed_InvalidWithNoExpiryDate_ShouldReturnFalse()
        {
            var accessToken = new AccessToken("accesstoken1", default(DateTime?), false);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.False);
        }

        [Test]
        public void CanAccessTokenBeUsed_InvalidAndNotExpired_ShouldReturnFalse()
        {
            var accessToken = new AccessToken("accesstoken1", _utcNow.AddDays(1d), false);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.False);
        }

        [Test]
        public void CanAccessTokenBeUsed_InvalidAndExpired_ShouldReturnFalse()
        {
            var accessToken = new AccessToken("accesstoken1", _utcNow.AddDays(-1), false);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.False);
        }

        [Test]
        public void CanAccessTokenBeUsed_ValidWithNoExpiryDate_ShouldReturnTrue()
        {
            var accessToken = new AccessToken("accesstoken1", default(DateTime?), true);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.True);
        }

        [Test]
        public void CanAccessTokenBeUsed_ValidAndNotExpired_ShouldReturnTrue()
        {
            var accessToken = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.True);
        }

        [Test]
        public void CanAccessTokenBeUsed_ValidAndExpiresWithin30Seconds_ShouldReturnFalse()
        {
            var accessToken = new AccessToken("accesstoken1", _utcNow.AddSeconds(29d), true);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.False);
        }

        [Test]
        public void CanAccessTokenBeUsed_ValidAndExpired_ShouldReturnFalse()
        {
            var accessToken = new AccessToken("accesstoken1", _utcNow.AddDays(-1), true);
            Assert.That(_classUnderTest.CanAccessTokenBeUsed(accessToken), Is.False);
        }

        [Test]
        public void InvalidateToken_SameWebApi_SameCredentialName_ShouldInvalidate()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            var accessToken2 = new AccessToken("accesstoken2", default(DateTime?), true);
            SetUpAccessTokens(accessToken1, accessToken2);
            _classUnderTest.InvalidateToken(_context.WebApiId, TestCredential.Frank.Name, GetToken(TestCredential.Frank));
            Assert.That(GetToken(TestCredential.Frank).AccessToken.Equals("accesstoken2"));
        }

        [Test]
        public void InvalidateToken_SameWebApi_DifferentCredentialName_ShouldNotInvalidate()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            SetUpAccessTokens(accessToken1, null);
            _classUnderTest.InvalidateToken(_context.WebApiId, TestCredential.Barry.Name, GetToken(TestCredential.Frank));
            Assert.That(GetToken(TestCredential.Frank).AccessToken.Equals("accesstoken1"));
        }

        [Test]
        public void InvalidateToken_DifferentWebApi_SameCredentialName_ShouldNotInvalidate()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            SetUpAccessTokens(accessToken1, null);
            _classUnderTest.InvalidateToken(Guid.NewGuid(), TestCredential.Frank.Name, GetToken(TestCredential.Frank));
            Assert.That(GetToken(TestCredential.Frank).AccessToken.Equals("accesstoken1"));
        }

        [Test]
        public void InvalidateToken_DifferentWebApi_DifferentCredentialName_ShouldNotInvalidate()
        {
            var accessToken1 = new AccessToken("accesstoken1", _utcNow.AddDays(1d), true);
            SetUpAccessTokens(accessToken1, null);
            _classUnderTest.InvalidateToken(Guid.NewGuid(), TestCredential.Barry.Name, GetToken(TestCredential.Frank));
            Assert.That(GetToken(TestCredential.Frank).AccessToken.Equals("accesstoken1"));
        }

        private void SetUpAccessTokens(AccessToken first, AccessToken second) => _requesterMock.SetupSequence(x => x.RequestAccessToken(_context, It.IsAny<ICredential>())).Returns(first).Returns(second);
    }
}
#endif
