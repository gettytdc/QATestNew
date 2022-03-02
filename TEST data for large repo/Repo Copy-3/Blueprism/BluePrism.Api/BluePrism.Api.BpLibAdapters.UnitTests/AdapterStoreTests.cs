namespace BluePrism.Api.BpLibAdapters.UnitTests
{
    using System.Threading.Tasks;
    using AutomateAppCore;
    using BpLibAdapters;
    using FluentAssertions;
    using Func;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    [TestFixture]
    public class AdapterStoreTests : UnitTestBase<AdapterStore<ITestAdapter>>
    {
        [Test]
        public async Task GetServerForToken_ShouldReturnServerInstance_WhenSuccessful()
        {
            GetMock<IServerStore>()
                .Setup(m => m.GetServerInstanceForToken("Test"))
                .Returns(() => Succeed(GetMock<IServer>().Object));

            var result = await ClassUnderTest.GetAdapterForToken("Test");

            result.Should().BeAssignableTo<Success<ITestAdapter>>();
        }

        [Test]
        public async Task GetAnonymousAdapter_ShouldReturnServerInstance_WhenSuccessful()
        {
            GetMock<IServerStore>()
                .Setup(m => m.GetUnkeyedServerInstance())
                .Returns(() => Succeed(GetMock<IServer>().Object));

            var result = await ClassUnderTest.GetAnonymousAdapter();

            result.Should().BeAssignableTo<Success<ITestAdapter>>();
        }
    }

    public interface ITestAdapter : IServerAdapter
    { }
}
