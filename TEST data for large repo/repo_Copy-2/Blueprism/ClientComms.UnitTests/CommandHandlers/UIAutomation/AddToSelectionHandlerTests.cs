using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class AddToSelectionHandlerTests : UIAutomationHandlerTestBase<AddToSelectionHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAAddToSelection");
            }
        }

        [Test]
        public void ExecuteWithValidElementReturnsOk()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithValidElementAddsToSelection()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
            Execute(Query);
            GetMock<ISelectionItemPattern>().Verify(m => m.AddToSelection(), Times.AtLeastOnce());
        }

        [Test]
        public void ExecuteWithInvalidElementThrowsPatternNotFound()
        {
            
            Assert.Throws<PatternNotFoundException<ISelectionItemPattern>>(()=>Execute(Query));
        }
    }
}