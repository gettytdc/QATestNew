using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetRadioCheckedStateHandlerTests : UIAutomationHandlerTestBase<GetRadioCheckedStateHandler>
    {
        [TestCase(true, "True")]
        [TestCase(false, "False")]
        public void Execute_WithElementSupportingPattern_ReturnsState(bool selected, string expected)
        {
            var patternMock = ElementMock.MockPattern<ISelectionItemPattern>();
            patternMock.Setup(p => p.CurrentIsSelected).Returns(selected);
            var reply = Execute(clsQuery.Parse("UIAGetRadioCheckedState"));
            Assert.That(reply.Message, Is.EqualTo(expected));
        }

        [Test]
        public void Execute_WithElementNotSupportingPattern_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<ISelectionItemPattern>>(() => Execute(clsQuery.Parse("UIAGetRadioCheckedState")));
        }
    }
}