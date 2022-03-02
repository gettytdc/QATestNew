using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetIsFocusedHandlerTests : UIAutomationHandlerTestBase<GetIsFocusedHandler>
    {
        private static clsQuery Query
        {
            get
            {
                return clsQuery.Parse("UIAGetExpanded");
            }
        }

        [Test]
        [TestCase(true, ExpectedResult = "True")]
        [TestCase(false, ExpectedResult = "False")]
        public string ExecuteReturnsExpectedResult(bool value)
        {
            ElementMock.SetupGet(m => m.CurrentHasKeyboardFocus).Returns(value);
            return Execute(Query).Message;
        }
    }
}