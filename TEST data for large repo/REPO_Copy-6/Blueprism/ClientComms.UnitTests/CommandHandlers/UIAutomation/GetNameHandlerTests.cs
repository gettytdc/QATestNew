using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetNameHandlerTests : UIAutomationHandlerTestBase<GetNameHandler>
    {
        private clsQuery Query => clsQuery.Parse("getname");

        [Test]
        public void ExecuteGetNameReturnsName()
        {
            const string name = "SlimShady";

            ElementMock.Setup(e => e.GetCurrentPropertyValue(PropertyType.Name)).Returns(name);

            var reply = Execute(Query);
            Assert.That(reply.Message, Is.EqualTo(name));
        }
    }
}
