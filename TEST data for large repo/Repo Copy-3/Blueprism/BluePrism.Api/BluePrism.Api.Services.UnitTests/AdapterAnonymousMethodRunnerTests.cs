namespace BluePrism.Api.Services.UnitTests
{
    using System.Threading.Tasks;
    using BpLibAdapters;
    using Func;
    using Moq;
    using NUnit.Framework;
    using Utilities.Testing;

    using static Func.ResultHelper;

    [TestFixture(Category = "Unit Test")]
    public class AdapterAnonymousMethodRunnerTests : UnitTestBase<AdapterAnonymousMethodRunner<TestAdapter>>
    {
        [Test]
        public async Task Execute_WithReturnValue_InvokesMethod()
        {
            GetMock<IAdapterStore<TestAdapter>>()
                .Setup(m => m.GetAnonymousAdapter())
                .ReturnsAsync(Succeed(new TestAdapter()));

            await ClassUnderTest.Execute(_ => Succeed(1234).ToTask());

            GetMock<IAdapterStore<TestAdapter>>()
                .Verify(m => m.GetAnonymousAdapter(), Times.Once);
        }
    }
}
