namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BluePrism.Api.Domain.Errors;
    using BluePrism.Server.Domain.Models;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class AuthServerAdapterTests : UnitTestBase<AuthServerAdapter>
    {
        [Test]
        public async Task GetAuthenticationServerUrl_ShouldReturnUrl_WhenSuccessful()
        {
            var url = "http://someurl.com";
            GetMock<IServer>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .Returns(url);
            var result = await ClassUnderTest.GetAuthenticationServerUrl();
            ((Success<string>)result).Value.Should().Be(url);
        }

        [Test]
        public async Task GetAuthenticationServerUrl_ShouldReturnSuccess_WhenSuccessful()
        {
            GetMock<IServer>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .Returns("http://someurl.com");
            var result = await ClassUnderTest.GetAuthenticationServerUrl();
            (result is Success).Should().BeTrue();
        }

        [Test]
        public async Task GetAuthenticationServerUrl_ShouldReturnFailure_WhenAuthenticationServerNotConfiguredExceptionThrown()
        {
            GetMock<IServer>()
                .Setup(m => m.GetAuthenticationServerUrl())
                .Throws<AuthenticationServerNotConfiguredException>();

            var result = await ClassUnderTest.GetAuthenticationServerUrl();

            (result is Failure<AuthServerNotConfiguredError>).Should().BeTrue();
        }
    }
}
