using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping;
using BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping.AuthenticationServer;
using BluePrism.UnitTesting.TestSupport;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.clsServerPartialClasses.AuthenticationServerUserMapping
{
    [TestFixture]
    public class AuthenticationServerHttpRequesterTests : UnitTestBase<AuthenticationServerHttpRequester>
    {
        private const string TestAuthenticationServerUrl = "https://ims:5000";

        private static readonly AuthenticationServerUser AuthenticationServerUser = new AuthenticationServerUser
        {
            Id = Guid.NewGuid(),
            Username = "john",
            FirstName = "John",
            LastName = "Wayne",
            Email = "john@email.com",
            CurrentPassword = string.Empty,
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };

        private static readonly UserMappingRecord BluePrismUserAuthenticationServerUserMappingRecord =
            new UserMappingRecord("john", null, "John", "Wayne", "john@email.com");

        private string _testAuthenticationServerUserJsonString;
        private WebException _unauthorisedWebException;
        private WebException _notFoundWebException;
        private clsCredential _testClientCredential;
        
        public override void Setup(Action<ContainerBuilder> container)
        {
            base.Setup(container);                        
            
            GetMock<IAccessTokenRequester>()
                .Setup(m => m.RequestAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SecureString>()))
                .ReturnsAsync(new BluePrism.AutomateProcessCore.WebApis.AccessTokens.AccessToken(string.Empty, DateTime.MaxValue, true));

            var unauthorisedWebResponse = new Mock<HttpWebResponse>();
            unauthorisedWebResponse.Setup(m => m.StatusCode).Returns(HttpStatusCode.Unauthorized);

            var notFoundWebResponse = new Mock<HttpWebResponse>();
            notFoundWebResponse.Setup(m => m.StatusCode).Returns(HttpStatusCode.NotFound);

            _unauthorisedWebException = new WebException("", null, WebExceptionStatus.UnknownError, unauthorisedWebResponse.Object);
            _notFoundWebException = new WebException("", null, WebExceptionStatus.UnknownError, notFoundWebResponse.Object);

            _testClientCredential = new clsCredential();

            _testAuthenticationServerUserJsonString =
                JsonConvert.SerializeObject(AuthenticationServerUser,
                    new JsonSerializerSettings
                    {
                        ContractResolver =
                            new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()}
                    });
        }

        [Test]
        public void Ctor_WithMissingHttpClientWrapper_ThrowsArgumentNullException()
        {
            Action ctorAction = () => new AuthenticationServerHttpRequester(null, GetMock<IAccessTokenRequester>().Object);

           ctorAction.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WithMissingAccessTokenRequester_ThrowsArgumentNullException()
        {
            Action ctorAction = () => new AuthenticationServerHttpRequester(GetMock<IHttpClientWrapper>().Object, null);

           ctorAction.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public async Task GetUser_WithValidRequest_ReturnsExpectedUser() =>
            await WhenFirstRequestIsSuccessful_ThenShouldReturnExpectedUser(async () => await ClassUnderTest.GetUser(AuthenticationServerUser.Id,
                TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task GetUser_FirstAttemptUnauthorised_ReturnsExpectedUser() =>
            await WhenFirstRequestUnauthorizedAndRetrySuccessful_ThenShoulReturnExpectedUser(async () =>
                await ClassUnderTest.GetUser(AuthenticationServerUser.Id, TestAuthenticationServerUrl,
                    _testClientCredential));     

        [Test]
        public async Task GetUser_UnauthorisedAttempts_ShouldOnlyTryTwice() =>
            await WhenFirstRequestUnauthorizedAndRetryAlsoUnauthorized_ThenShouldNotRetryAgain(async () => await ClassUnderTest.GetUser(AuthenticationServerUser.Id,
                TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task GetUser_FirstAttemptNotFound_ShouldNotRetry() =>
            await WhenUserNotFound_ThenShouldNotRetry(async () => await ClassUnderTest.GetUser(AuthenticationServerUser.Id,
                TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task GetUser_AccessTokenExpired_ShouldGetNewAccessToken() =>
            await WhenAccessTokenExpired_ThenNewAccessTokenShouldBeRequested(async () => await ClassUnderTest.GetUser(AuthenticationServerUser.Id,
                TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task PostUser_WithValidRequest_ReturnsExpectedUser() =>
            await WhenFirstRequestIsSuccessful_ThenShouldReturnExpectedUser(async () =>
                await ClassUnderTest.PostUser(BluePrismUserAuthenticationServerUserMappingRecord,
                    TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task PostUser_FirstAttemptUnauthorised_ReturnsExpectedUser() =>
            await WhenFirstRequestUnauthorizedAndRetrySuccessful_ThenShoulReturnExpectedUser(async () =>
                await ClassUnderTest.PostUser(BluePrismUserAuthenticationServerUserMappingRecord,
                    TestAuthenticationServerUrl, _testClientCredential));


        [Test]
        public async Task PostUser_UnauthorisedAttempts_ShouldOnlyTryTwice() =>
            await WhenFirstRequestUnauthorizedAndRetryAlsoUnauthorized_ThenShouldNotRetryAgain(async () =>
                await ClassUnderTest.PostUser(BluePrismUserAuthenticationServerUserMappingRecord,
                    TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task PostUser_FirstAttemptNotFound_ShouldNotRetry() =>
            await WhenUserNotFound_ThenShouldNotRetry(async () =>
                await ClassUnderTest.PostUser(BluePrismUserAuthenticationServerUserMappingRecord,
                    TestAuthenticationServerUrl, _testClientCredential));

        [Test]
        public async Task PostUser_AccessTokenExpired_ShouldGetNewAccessToken() =>
            await WhenAccessTokenExpired_ThenNewAccessTokenShouldBeRequested(async () =>
                await ClassUnderTest.PostUser(BluePrismUserAuthenticationServerUserMappingRecord,
                    TestAuthenticationServerUrl, _testClientCredential));
        
        public async Task WhenFirstRequestIsSuccessful_ThenShouldReturnExpectedUser(Func<Task<AuthenticationServerUser>> apiCall)
        {
           GetMock<IHttpClientWrapper>().Setup(m =>
                    m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_testAuthenticationServerUserJsonString, Encoding.UTF8, "application/json")
                });

            var result = await apiCall.Invoke();

            result.ShouldBeEquivalentTo(AuthenticationServerUser);
        }

        public async Task WhenFirstRequestUnauthorizedAndRetrySuccessful_ThenShoulReturnExpectedUser(Func<Task<AuthenticationServerUser>> apiCall)
        {
           GetMock<IHttpClientWrapper>().SetupSequence(m =>
                    m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(_unauthorisedWebException)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_testAuthenticationServerUserJsonString, Encoding.UTF8, "application/json")
                });

            var result = await apiCall.Invoke();

            result.ShouldBeEquivalentTo(AuthenticationServerUser);
        }

        private async Task WhenFirstRequestUnauthorizedAndRetryAlsoUnauthorized_ThenShouldNotRetryAgain(Func<Task<AuthenticationServerUser>> apiCall)
        {
           GetMock<IHttpClientWrapper>().SetupSequence(m =>
                    m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(_unauthorisedWebException)
                .ThrowsAsync(_unauthorisedWebException);

           await apiCall.Invoke();

            GetMock<IHttpClientWrapper>().Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(),
                It.IsAny<HttpCompletionOption>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        private async Task WhenUserNotFound_ThenShouldNotRetry(Func<Task<AuthenticationServerUser>> apiCall)
        {
           GetMock<IHttpClientWrapper>().SetupSequence(m =>
                    m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(_notFoundWebException);

           await apiCall.Invoke();

            GetMock<IHttpClientWrapper>().Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(),
                It.IsAny<HttpCompletionOption>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        private async Task WhenAccessTokenExpired_ThenNewAccessTokenShouldBeRequested(Func<Task<AuthenticationServerUser>> apiCall)
        {
            var expiredAccessToken = new BluePrism.AutomateProcessCore.WebApis.AccessTokens.AccessToken("expiredAccessToken", DateTime.MinValue, false);
            ReflectionHelper.SetPrivateField("mPreviousAccessToken", ClassUnderTest, expiredAccessToken);

           GetMock<IHttpClientWrapper>().SetupSequence(m =>
                m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(),
                    It.IsAny<CancellationToken>()));

           await apiCall.Invoke();

            GetMock<IAccessTokenRequester>().Verify(
                m => m.RequestAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SecureString>()),
                Times.Once);
        }
    }
}
