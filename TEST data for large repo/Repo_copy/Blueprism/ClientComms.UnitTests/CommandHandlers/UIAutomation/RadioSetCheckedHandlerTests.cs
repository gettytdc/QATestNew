using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class RadioSetCheckedHandlerTests : UIAutomationHandlerTestBase<RadioSetCheckedHandler>
    {
        [Test]
        public void Execute_WithElementUsingPattern_ShouldSelect()
        {
            var patternMock = ElementMock.MockPattern<ISelectionItemPattern>();
            var reply = Execute(clsQuery.Parse("UIARadioSetChecked newtext=True"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            patternMock.Verify(p => p.Select());
        }

        [Test]
        public void Execute_WithElementUsingPattern_ShouldDoNothing()
        {
            var patternMock = ElementMock.MockPattern<ISelectionItemPattern>();
            patternMock.Setup(r => r.CurrentIsSelected).Returns(false);
            var reply = Execute(clsQuery.Parse("UIARadioSetChecked newtext=False"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            Assert.That(patternMock.Object.CurrentIsSelected, Is.EqualTo(false));
        }

        [Test]
        public void Execute_SendingFalse_ShouldDoNothing()
        {
            // Test written to document behaviour, currently sending a value of False
            // does nothing - the only way to deselect is to select another item
            var patternMock = ElementMock.MockPattern<ISelectionItemPattern>();
            patternMock.Setup(r => r.CurrentIsSelected).Returns(true);
            var reply = Execute(clsQuery.Parse("UIARadioSetChecked newtext=False"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            Assert.That(patternMock.Object.CurrentIsSelected, Is.EqualTo(true));
        }

        [Test]
        public void Execute_WithElementNotImplementingPatterns_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<ISelectionItemPattern>>(() => Execute(clsQuery.Parse("UIARadioSetChecked newtext=True")));
        }
    }
}