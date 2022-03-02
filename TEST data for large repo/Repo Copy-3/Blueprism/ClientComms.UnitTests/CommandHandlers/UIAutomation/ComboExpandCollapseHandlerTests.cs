using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Conditions;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ComboExpandCollapseHandlerTests : UIAutomationHandlerTestBase<ComboExpandCollapseHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAComboExpandCollapse");
            }
        }

        [Test]
        public void ExecuteWithExpandCollapsePatternReturnsOk()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<IExpandCollapsePattern>()).Returns(GetMock<IExpandCollapsePattern>().Object);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithExpandCollapsePatternCallsExpandCollapse()
        {
            ElementMock.Setup(m => m.GetCurrentPattern<IExpandCollapsePattern>()).Returns(GetMock<IExpandCollapsePattern>().Object);
            Execute(Query);
            GetMock<IExpandCollapsePattern>().Verify(m => m.ExpandCollapse(), Times.Once());
        }

        [Test]
        public void ExecuteWithButtonReturnsOk()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.Setup(m => m.GetCurrentPattern<IInvokePattern>()).Returns(GetMock<IInvokePattern>().Object);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithButtonCallsInvoke()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.Setup(m => m.GetCurrentPattern<IInvokePattern>()).Returns(GetMock<IInvokePattern>().Object);
            Execute(Query);
            GetMock<IInvokePattern>().Verify(m => m.Invoke(), Times.Once());
        }

        [Test]
        public void ExecuteWithInvalidElementThrowsPatternNotFound()
        {
            Assert.Throws<PatternNotFoundException<IExpandCollapsePattern>>(() => Execute(Query));
        }
    }
}