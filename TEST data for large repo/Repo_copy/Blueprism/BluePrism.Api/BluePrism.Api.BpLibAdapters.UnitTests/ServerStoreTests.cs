namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class ServerStoreTests : UnitTestBase<ServerStore>
    {
        [Test]
        public void GetUnkeyedServerInstance_ReturnsServerInstance()
        {
            var mockServer = GetMock<IServer>().Object;

            GetMock<IBluePrismServerFactory>()
                .Setup(m => m.ClientInit())
                .Returns(mockServer);

            var result = ClassUnderTest.GetUnkeyedServerInstance();

            result.ValueOr(() => default(IServer)).Should().BeSameAs(mockServer);
        }

        [Test]
        public void GetServerInstance_ReturnsNewServerInstance_WhenServerNotPreviouslyCreated()
        {
            GetMock<IUserStaticMethodWrapper>()
                .Setup(m => m.LoginWithAccessToken("API", "testkey", "en-us", It.IsAny<IServer>()))
                .Returns(new LoginResult(LoginResultCode.Success));

            ClassUnderTest.GetServerInstanceForToken("testkey");

            GetMock<IBluePrismServerFactory>()
                .Verify(m => m.ClientInit(), Times.Once);
        }

        [Test]
        public void GetServerInstance_ReturnsExistingServerInstance_WhenServerPreviouslyCreated()
        {
            var expectedMockServer = GetMock<IServer>().Object;
            var incorrectMockServer = new Mock<IServer>().Object;

            GetMock<IBluePrismServerFactory>()
                .SetupSequence(m => m.ClientInit())
                .Returns(expectedMockServer)
                .Returns(incorrectMockServer);

            GetMock<IUserStaticMethodWrapper>()
                .Setup(m => m.LoginWithAccessToken("API", "testkey", "en-us", It.IsAny<IServer>()))
                .Returns(new LoginResult(LoginResultCode.Success));

            _ = ClassUnderTest.GetServerInstanceForToken("testkey");

            var result = ClassUnderTest.GetServerInstanceForToken("testkey");

            result.ValueOr(() => default(IServer)).Should().BeSameAs(expectedMockServer);
        }

        [Test]
        public void GetServerInstance_ReturnsDifferentServerInstances_WhenKeyDiffers()
        {
            var firstMockServer = GetMock<IServer>().Object;
            var secondMockServer = new Mock<IServer>().Object;

            GetMock<IBluePrismServerFactory>()
                .SetupSequence(m => m.ClientInit())
                .Returns(firstMockServer)
                .Returns(secondMockServer);

            GetMock<IUserStaticMethodWrapper>()
                .Setup(m => m.LoginWithAccessToken("API", It.Is<string>(x => x.StartsWith("testkey")), "en-us", It.IsAny<IServer>()))
                .Returns(new LoginResult(LoginResultCode.Success));

            var result1 = ClassUnderTest.GetServerInstanceForToken("testkey1");

            var result2 = ClassUnderTest.GetServerInstanceForToken("testkey2");

            result1.ValueOr(() => default(IServer)).Should().BeSameAs(firstMockServer);
            result2.ValueOr(() => default(IServer)).Should().BeSameAs(secondMockServer);
        }

        [Test]
        public void GetServerInstance_ReturnsBluePrismUnauthenticatedError_WhenLoginFails()
        {
            GetMock<IUserStaticMethodWrapper>()
                .Setup(m => m.LoginWithAccessToken("API", "testkey", "en-us", It.IsAny<IServer>()))
                .Returns(new LoginResult(LoginResultCode.BadCredentials));

            var result = ClassUnderTest.GetServerInstanceForToken("testkey");

            result.Should().BeAssignableTo<Failure<BluePrismUnauthenticatedError>>();
        }

        [Test]
        public void CloseServerInstance_RemovesInstanceFromCollection()
        {
            var firstMockServer = GetMock<IServer>().Object;
            var secondMockServer = new Mock<IServer>().Object;

            GetMock<IUserStaticMethodWrapper>()
                .Setup(m => m.LoginWithAccessToken("API", "testkey", "en-us", It.IsAny<IServer>()))
                .Returns(new LoginResult(LoginResultCode.Success));

            GetMock<IBluePrismServerFactory>()
                .SetupSequence(m => m.ClientInit())
                .Returns(firstMockServer)
                .Returns(secondMockServer);

            _ = ClassUnderTest.GetServerInstanceForToken("testkey");

            ClassUnderTest.CloseServerInstance("testkey");

            var result = ClassUnderTest.GetServerInstanceForToken("testkey");

            result.ValueOr(() => default(IServer)).Should().BeSameAs(secondMockServer);
        }
    }
}
