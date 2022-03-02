using NUnit.Framework;
using FluentAssertions;
using BluePrism.AutomateAppCore.EnvironmentFunctions;
using BluePrism.AutomateProcessCore;
using BluePrism.AutomateAppCore.Config;
using BluePrism.Utilities.Testing;

namespace AutomateAppCore.UnitTests.EnvironmentFunctions
{
    [TestFixture]
    public class GetConnectionNameFunctionTests : UnitTestBase<GetConnectionNameFunction>
    {
        private static string TestConnectionName => "Fred";

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            GetMock<IOptions>().Setup(o => o.CurrentConnectionName).Returns(TestConnectionName);
        }

        [Test]
        public void Evaluate_WithSession_ShouldReturnSessionIdString()
        {            
            var connectionNameValue = ClassUnderTest.Evaluate(null, null);

            connectionNameValue.Should().Be(new clsProcessValue(TestConnectionName));
        }
    }
}