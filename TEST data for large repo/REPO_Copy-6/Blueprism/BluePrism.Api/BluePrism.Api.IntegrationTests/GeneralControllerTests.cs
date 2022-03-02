namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Autofac;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using AutomateAppCore.Resources;
    using BpLibAdapters;
    using CommonTestClasses;
    using CommonTestClasses.Extensions;
    using ControllerClients;
    using FluentAssertions;
    using Func;
    using IdentityModel;
    using Logging;
    using Models;
    using Moq;
    using NUnit.Framework;
    using ResourceParameters = Server.Domain.Models.ResourceParameters;
    using ResourceAttribute = Models.ResourceAttribute;

    [TestFixture]
    public class GeneralControllerTests : ControllerTestBase<GeneralControllerClient>
    {
        [SetUp]
        public override void Setup() =>
            Setup(() =>
            {
                GetMock<IServer>()
                    .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReloginTokenRequest>()))
                    .Returns(new LoginResultWithReloginToken(LoginResultCode.Success));

                GetMock<IBluePrismServerFactory>()
                    .Setup(m => m.ClientInit())
                    .Returns(() => GetMock<IServer>().Object);

                RegisterMocks(builder =>
                {
                    builder.Register(_ => GetMock<IBluePrismServerFactory>().Object);
                    return builder;
                });
            });

        [Test]
        public async Task WhenHttpNoContentStatusCodeIsReturned_BodyShouldBeEmpty()
        {
            const int emptyBodyContentLength = 0;
            var (id, data) = SetupMockDataForHttpNoContent();

            var result = await Subject.MakeRequestThatReturnsHttpNoContent(id, data, AuthHelper.GenerateToken(TestIssuer, TestAudience));

            result.StatusCode.Should().Be(HttpStatusCode.NoContent);
            result.Content.Headers.ContentLength.Should().Be(emptyBodyContentLength);
        }

        [Test]
        public async Task Controllers_WhenUnexpectedExceptionOccurs_ShouldReturn500()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourceInfo(It.IsAny<Core.Resources.ResourceAttribute>(), It.IsAny<Core.Resources.ResourceAttribute>(), It.IsAny<string>()))
                .Throws<UnexpectedTestException>();

            var result = await Subject.MakeRequestToAuthorizeEndpoint(AuthHelper.GenerateToken(TestIssuer, TestAudience));

            result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task Controllers_WhenUnexpectedExceptionOccurs_ShouldReturnEmptyJsonObject()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourceInfo(It.IsAny<Core.Resources.ResourceAttribute>(), It.IsAny<Core.Resources.ResourceAttribute>(), It.IsAny<string>()))
                .Throws<UnexpectedTestException>();

            var result = await Subject.MakeRequestToAuthorizeEndpoint(AuthHelper.GenerateToken(TestIssuer, TestAudience));

            var resultContent = await result.Content.ReadAsStringAsync();

            resultContent.Should().Be("{}");
        }

        [Test]
        public async Task Controllers_WhenUnexpectedExceptionOccurs_ShouldLogError()
        {
            var testException = new UnexpectedTestException();

            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<ResourceParameters>()))
                .Throws(testException);

            await Subject.MakeRequestToAuthorizeEndpoint(AuthHelper.GenerateToken(TestIssuer, TestAudience));

            GetMock<ILogger<ApiExceptionResponseConverter>>()
                .Verify(m => m.Error(AssertUnexpectedTestException(testException), It.IsAny<string>()), Times.Once);
        }

        private static AggregateException AssertUnexpectedTestException(UnexpectedTestException testException) =>
            It.Is<AggregateException>(x => ((UnexpectedTestException)x.GetBaseException()).Id.Equals(testException.Id));

        [Test]
        public async Task WhenValidTokenProvided_ShouldReturnOk()
        {
            GetMock<IServer>()
                .Setup(m => m.GetResourcesData(It.IsAny<ResourceParameters>()))
                .Returns(Array.Empty<ResourceInfo>());
            var result = await Subject.MakeRequestToAuthorizeEndpoint(AuthHelper.GenerateToken(TestIssuer, TestAudience));
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        [TestCaseSource(nameof(InvalidTokens))]
        public async Task WhenInvalidTokenProvided_ShouldReturnUnauthorizedCode(string token)
        {
            var result = await Subject.MakeRequestToAuthorizeEndpoint(token);
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task WhenInvalidNickName_ShouldReturnUnauthorizedCode()
        {
            var token = AuthHelper.GenerateToken(TestIssuer, TestAudience,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(1), new [] { new Claim(JwtClaimTypes.Id, Guid.NewGuid().ToString()) });
            GetMock<IServer>()
                .Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>(), token, It.IsAny<ReloginTokenRequest>()))
                .Returns(new LoginResultWithReloginToken(LoginResultCode.UnableToFindUser));

            var result = await Subject.MakeRequestToAuthorizeEndpoint(token);
            result.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.Unauthorized);
        }

        private static IEnumerable<TestCaseData> InvalidTokens => new[]
        {
            string.Empty,
            AuthHelper.GenerateToken("https://invalid-issuer.com", TestAudience),
            AuthHelper.GenerateToken(TestIssuer, TestAudience, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1)),
            AuthHelper.GenerateToken(TestIssuer, "invalid-audience", DateTime.UtcNow, DateTime.UtcNow.AddDays(1))
        }.ToTestCaseData();

        private (Guid Id, object data) SetupMockDataForHttpNoContent()
        {
            var resourceId = Guid.NewGuid();
            var resource = new ResourceInfo { ID = resourceId, Attributes = Core.Resources.ResourceAttribute.None };
            var updateResource = new UpdateResourceModel { Attributes = new[] { ResourceAttribute.Retired } };

            GetMock<IServer>().Setup(m => m.GetResourceData(It.IsAny<Guid>()))
                .Returns(resource);

            return (resourceId, updateResource);
        }
    }

    public class UnexpectedTestException : Exception
    {
        public Guid Id { get; } = Guid.NewGuid();

        public override string ToString() =>
            $"Test exception: {Id}";
    }
}
