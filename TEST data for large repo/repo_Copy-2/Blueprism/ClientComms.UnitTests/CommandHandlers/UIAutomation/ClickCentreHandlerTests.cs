using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.Operations;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ClickCentreHandlerTests : UIAutomationHandlerTestBase<ClickCentreHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAClickCentre");
            }
        }

        [Test]
        public void ExecuteReturnsOk()
        {
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteClicks()
        {
            Execute(Query);
            GetMock<IMouseOperationsProvider>().Verify(m => m.ClickAt(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<MouseButton>()), Times.Once());
        }
    }
}