namespace BluePrism.Api.Services.UnitTests
{
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Domain.Errors;
    using FluentAssertions;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    public class TestAdapter : IServerAdapter
    {
    }

    [TestFixture]
    public class AdapterAuthenticatedMethodRunnerTests : UnitTestBase<AdapterAuthenticatedMethodRunner<TestAdapter>>
    {
        [Test]
        public async Task ExecuteForUser_PassesCredentials()
        {
            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAdapterForToken(It.IsAny<string>()))
                .ReturnsAsync(Succeed(new TestAdapter()));

            await ClassUnderTest.ExecuteForUser(_ => Succeed().ToTask());

            GetMock<IAdapterStore<TestAdapter>>()
                .Verify(m => m.GetAdapterForToken(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ExecuteForUser_WithReturnValue_PassesCredentials()
        {
            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAdapterForToken(It.IsAny<string>()))
                .ReturnsAsync(Succeed(new TestAdapter()));

            await ClassUnderTest.ExecuteForUser(_ => Succeed(1234).ToTask());

            GetMock<IAdapterStore<TestAdapter>>()
                .Verify(m => m.GetAdapterForToken(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task ExecuteForUser_OnValidCredentials_ReturnsValueFromMethod()
        {
            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAdapterForToken(It.IsAny<string>()))
                .ReturnsAsync(Succeed(new TestAdapter()));

            var result = await ClassUnderTest.ExecuteForUser(_ => Succeed(1234).ToTask());

            ((Success<int>)result).Value.Should().Be(1234);
        }

        [Test]
        public async Task ExecuteForUser_OnInvalidCredentials_DoesNotInvokeMethod()
        {
            var hasBeenInvoked = false;
            Task<Result> TestMethod(TestAdapter _)
            {
                hasBeenInvoked = true;
                return Succeed().ToTask();
            }

            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAdapterForToken(It.IsAny<string>()))
                .ReturnsAsync(ResultHelper<TestAdapter>.Fail(new BluePrismUnauthenticatedError("Test")));

            await ClassUnderTest.ExecuteForUser(TestMethod);

            hasBeenInvoked.Should().BeFalse();
        }

        [Test]
        public async Task ExecuteForUser_WithReturnValue_OnInvalidCredentials_DoesNotInvokeMethod()
        {
            var hasBeenInvoked = false;
            Task<Result<int>> TestMethod(TestAdapter _)
            {
                hasBeenInvoked = true;
                return Succeed(1234).ToTask();
            }

            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAdapterForToken(It.IsAny<string>()))
                .ReturnsAsync(ResultHelper<TestAdapter>.Fail(new BluePrismUnauthenticatedError("Test")));

            await ClassUnderTest.ExecuteForUser(TestMethod);

            hasBeenInvoked.Should().BeFalse();
        }
    }
}
