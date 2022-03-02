using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Conditions;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ComboSelectHandlerTests : UIAutomationHandlerTestBase<ComboSelectHandler>
    {
        [Test]
        public void ExecuteWithChildListReturnsOk()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.MockPattern<ISelectionItemPattern>();
            ElementMock.MockPattern<IExpandCollapsePattern>();
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>())).Returns(new[] { ElementMock.Object });
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(true);
            var result = Execute(clsQuery.Parse("UIAComboSelect index=1"));
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteSelectsItem()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>())).Returns(new[] { ElementMock.Object });
            var selectionItemMock = ElementMock.MockPattern<ISelectionItemPattern>();
            ElementMock.MockPattern<IExpandCollapsePattern>();
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(true);
            Execute(clsQuery.Parse("UIAComboSelect index=1"));
            selectionItemMock.Verify(m => m.Select(), Times.Once());
        }

        [Test]
        public void ExecuteWithExpandCollapsePatternCallsExpandCollapse()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.SetupSequence(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>()))
                .Returns(new List<IAutomationElement>()).Returns(new[] { ElementMock.Object }).Returns(new[] { ElementMock.Object });
            ElementMock.MockPattern<ISelectionItemPattern>();
            var expandCollapseMock = ElementMock.MockPattern<IExpandCollapsePattern>();
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(true);
            Execute(clsQuery.Parse("UIAComboSelect index=1"));
            expandCollapseMock.Verify(m => m.ExpandCollapse(), Times.Once());
        }

        [Test]
        public void ExecuteWithButtonReturnsOk()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.SetupSequence(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>())).Returns(new List<IAutomationElement>()).Returns(new[] { ElementMock.Object }).Returns(new[] { ElementMock.Object });
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(true);
            ElementMock.MockPattern<ISelectionItemPattern>();
            ElementMock.MockPattern<IInvokePattern>();
            var result = Execute(clsQuery.Parse("UIAComboSelect index=1"));
            Assert.AreEqual(Reply.Ok, result);
        }

        [Test]
        public void ExecuteWithButtonCallsInvoke()
        {
            ElementMock.Setup(m => m.FindFirst(It.IsAny<TreeScope>(), It.IsAny<IAutomationCondition>())).Returns(ElementMock.Object);
            ElementMock.Setup(m => m.FindAll(It.IsAny<TreeScope>(), It.IsAny<IAutomationCacheRequest>())).Returns(new[] { ElementMock.Object });
            ElementMock.Setup(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionItemPattern))).Returns(true);
            ElementMock.SetupSequence(m => m.PatternIsSupported(It.Is<PatternType>(x => x == PatternType.SelectionPattern))).Returns(false).Returns(true);
            ElementMock.MockPattern<ISelectionItemPattern>();
            var invokePatternMock = ElementMock.MockPattern<IInvokePattern>();
            Execute(clsQuery.Parse("UIAComboSelect index=1"));
            invokePatternMock.Verify(m => m.Invoke(), Times.Exactly(2));
        }

        [Test]
        public void ExecuteWithInvalidElementThrowsPatternNotFound()
        {
            Assert.Throws<PatternNotFoundException<IExpandCollapsePattern>>(() => Execute(clsQuery.Parse("UIAComboSelect index=1")));
        }
    }
}