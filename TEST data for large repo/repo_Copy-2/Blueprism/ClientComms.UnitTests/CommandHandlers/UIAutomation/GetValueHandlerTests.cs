using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetValueHandlerTests : UIAutomationHandlerTestBase<GetValueHandler>
    {
        [Test]
        public void Execute_WithElementUsingValuePattern_ShouldReturnValue()
        {
            var patternMock = ElementMock.MockPattern<IValuePattern>();
            patternMock.Setup(p => p.CurrentValue).Returns("test1");
            var reply = Execute(clsQuery.Parse("uiagetvalue"));
            Assert.That(reply.Message, Is.EqualTo("test1"));
        }

        [Test]
        public void Execute_WithElementUsingTextChildPattern_ShouldReturnValue()
        {
            var patternMock = ElementMock.MockPattern<ITextChildPattern>();
            patternMock.Setup(p => p.TextRange.GetText(int.MaxValue)).Returns("test1");
            var reply = Execute(clsQuery.Parse("uiagetvalue"));
            Assert.That(reply.Message, Is.EqualTo("test1"));
        }

        [Test]
        public void Execute_WithElementUsingTextPattern_ShouldReturnValue()
        {
            var patternMock = ElementMock.MockPattern<ITextPattern>();
            patternMock.Setup(p => p.DocumentRange.GetText(int.MaxValue)).Returns("test1");
            var reply = Execute(clsQuery.Parse("uiagetvalue"));
            Assert.That(reply.Message, Is.EqualTo("test1"));
        }

        [Test]
        public void Execute_WithElementNotImplementingPatterns_ShouldReturnElementName()
        {
            ElementMock.Setup(e => e.CurrentName).Returns("name1");
            var reply = Execute(clsQuery.Parse("uiagetvalue"));
            Assert.That(reply.Message, Is.EqualTo("name1"));
        }
    }
}