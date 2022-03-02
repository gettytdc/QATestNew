namespace BluePrism.Api.Services.UnitTests
{
    using System.Threading.Tasks;
    using Autofac;
    using BluePrism.Api.CommonTestClasses.Extensions;
    using BluePrism.Api.Domain.Errors;
    using BluePrism.Logging;
    using BpLibAdapters;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    [TestFixture(Category = "Unit Test")]
    public class AuthServiceTests : UnitTestBase<AuthService>
    {
        public override void Setup() => base.Setup(builder =>
        {
            builder.RegisterGeneric(typeof(MockAdapterAnonymousMethodRunner<>)).As(typeof(IAdapterAnonymousMethodRunner<>));
        });

        [Test]
        public async Task GetAuthServerUrl_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IAuthServerAdapter>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .ReturnsAsync(Succeed("https://someurl.com"));

            var result = await ClassUnderTest.GetAuthServerUrl();

            (result is Success<string>).Should().BeTrue();
        }

        [Test]
        public async Task GetAuthServerUrl_ShouldReturnUrl_WhenSuccessful()
        {
            var url = "https://someurl.com";
            GetMock<IAuthServerAdapter>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .ReturnsAsync(Succeed(url));

            var result = await ClassUnderTest.GetAuthServerUrl();

            ((Success<string>)result).Value.ShouldBeEquivalentTo(url);
        }

        [Test]
        public async Task GetAuthServerUrl_ShouldReturnFailure_WhenAuthServerHasNotBeenConfigured()
        {
            GetMock<IAuthServerAdapter>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .ReturnsAsync(ResultHelper<string>.Fail<AuthServerNotConfiguredError>());

            var result = await ClassUnderTest.GetAuthServerUrl();

            (result is Failure<AuthServerNotConfiguredError>).Should().BeTrue();
        }

        [Test]
        public async Task GetAuthServerUrl_ShouldLogInfo_WhenAuthServerHasNotBeenConfigured()
        {
            GetMock<IAuthServerAdapter>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .ReturnsAsync(ResultHelper<string>.Fail<AuthServerNotConfiguredError>());

            _ = await ClassUnderTest.GetAuthServerUrl();

            GetMock<ILogger<AuthService>>().ShouldLogInfoMessages();
        }
    }
}
