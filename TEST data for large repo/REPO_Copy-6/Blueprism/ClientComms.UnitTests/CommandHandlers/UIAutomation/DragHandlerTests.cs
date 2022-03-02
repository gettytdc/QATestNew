using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.Operations;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class DragHandlerTests : UIAutomationHandlerTestBase<DragHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIADrag targx=100 targy=200");
            }
        }

        [Test]
        public void ExecuteReturnsOk()
        {
            ElementMock.SetupGet(m => m.CurrentBoundingRectangle).Returns(new System.Drawing.Rectangle(100, 100, 100, 100));
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteBeginsDrag()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
            Execute(Query);
            GetMock<IMouseOperationsProvider>().Verify(m => m.DragFrom(It.Is<int>(x => x == 100), It.Is<int>(x => x == 200)), Times.Once());
        }
    }
}