using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetExpandedHandlerTests : UIAutomationHandlerTestBase<GetExpandedHandler>
    {
        private clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetExpanded");
            }
        }

        [TestCase(ExpandCollapseState.Expanded, ExpectedResult = "True")]
        [TestCase(ExpandCollapseState.PartiallyExpanded, ExpectedResult = "True")]
        [TestCase(ExpandCollapseState.LeafNode, ExpectedResult = "True")]
        [TestCase(ExpandCollapseState.Collapsed, ExpectedResult = "False")]
        public string ExecuteWithExpandCollapseElementReturnsCorrectValue(ExpandCollapseState state)
        {
            var expandCollapseMock = ElementMock.MockPattern<IExpandCollapsePattern>();
            expandCollapseMock.SetupGet(m => m.CurrentExpandCollapseState).Returns(state);
            return Execute(Query).Message;
        }

        [TestCase(ControlType.List, true, ExpectedResult = "True")]
        [TestCase(ControlType.List, false, ExpectedResult = "False")]
        [TestCase(ControlType.Menu, true, ExpectedResult = "True")]
        [TestCase(ControlType.Menu, false, ExpectedResult = "False")]
        [TestCase(ControlType.Custom, true, ExpectedResult = "False")]
        [TestCase(ControlType.Custom, false, ExpectedResult = "False")]
        public string ExecuteWithChildReturnsCorrectValue(ControlType control, bool visible)
        {
            ElementMock.Setup(m => m.FindAll(It.Is<TreeScope>(x => x == TreeScope.Children), It.Is<ControlType>(x => x == control))).Returns(new[] { ElementMock.Object });
            ElementMock.SetupGet(m => m.CurrentIsOffscreen).Returns(!visible);
            return Execute(Query).Message;
        }
    }
}