using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.Operations;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class MouseClickHandlerTests : UIAutomationHandlerTestBase<MouseClickHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAMouseClick newtext=asd targx=1 targy=2");
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
            ElementMock.Setup(e => e.CurrentBoundingRectangle).Returns(new System.Drawing.Rectangle(10, 10, 20, 20));
            GetMock<IMouseOperationsProvider>().Setup(e => e.ParseMouseButton("asd")).Returns(MouseButton.Left);
            Execute(Query);
            GetMock<IMouseOperationsProvider>().Verify(m => m.ClickAt(11, 12, false, MouseButton.Left), Times.Once());
        }
    }
}