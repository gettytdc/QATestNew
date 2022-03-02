using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ButtonPressHandlerTests : UIAutomationHandlerTestBase<ButtonPressHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAButtonPress");
            }
        }

        [Test]
        public void ExecuteWithInvokePatternReturnsOk()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.InvokePattern))).Returns(GetMock<IInvokePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.InvokePattern))).Returns(true);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithInvokePatternCallsInvoke()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.InvokePattern))).Returns(GetMock<IInvokePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.InvokePattern))).Returns(true);
            Execute(Query);
            GetMock<IInvokePattern>().Verify(m => m.Invoke(), Times.AtLeastOnce());
        }

        [Test]
        public void ExecuteWithTogglePatternReturnsOk()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.TogglePattern))).Returns(GetMock<ITogglePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.TogglePattern))).Returns(true);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithTogglePatternCallsInvoke()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.TogglePattern))).Returns(GetMock<ITogglePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.TogglePattern))).Returns(true);
            Execute(Query);
            GetMock<ITogglePattern>().Verify(m => m.Toggle(), Times.AtLeastOnce());
        }

        [Test]
        public void ExecuteWithExpandCollapsePatternReturnsOk()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.ExpandCollapsePattern))).Returns(GetMock<IExpandCollapsePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.ExpandCollapsePattern))).Returns(true);
            var result = Execute(Query);
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithExpandCollapsePatternCallsExpandCollapse()
        {
            ElementMock.Setup(m => m.GetCurrentPattern(It.Is<PatternType>(x => x == PatternType.ExpandCollapsePattern))).Returns(GetMock<IExpandCollapsePattern>().Object);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(p => p == PatternType.ExpandCollapsePattern))).Returns(true);
            Execute(Query);
            GetMock<IExpandCollapsePattern>().Verify(m => m.ExpandCollapse(), Times.AtLeastOnce());
        }

        [Test]
        public void ExecuteWithInvalidElementThrowsPatternNotFound()
        {
            Assert.Throws<PatternNotFoundException<IInvokePattern>>(() => Execute(Query));
        }
    }
}