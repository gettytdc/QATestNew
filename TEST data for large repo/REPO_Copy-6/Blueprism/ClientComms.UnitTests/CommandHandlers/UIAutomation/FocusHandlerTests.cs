using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.Operations;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class FocusHandlerTests : UIAutomationHandlerTestBase<FocusHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAFocus");
            }
        }

        [Test]
        public void ExecuteReturnsOk()
        {
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteCallsForceForeground()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
            Execute(Query);
            GetMock<IWindowOperationsProvider>().Verify(m => m.ForceForeground(It.IsAny<IntPtr>()), Times.Once());
        }

        [Test]
        public void ExecuteCallsSetFocus()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<ISelectionItemPattern>()).Returns(GetMock<ISelectionItemPattern>().Object);
            Execute(Query);
            ElementMock.Verify(m => m.SetFocus(), Times.Once());
        }
    }
}