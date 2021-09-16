using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableRowCountHandlerTests : UIAutomationHandlerTestBase<TableRowCountHandler>
    {
        [Test]
        public void Execute_ElementWithGridPattern_ShouldReturnValue()
        {
            var patternMock = ElementMock.MockPattern<IGridPattern>();
            patternMock.Setup(p => p.CurrentRowCount).Returns(10000);
            var reply = Execute(clsQuery.Parse("uiatablerowcount"));
            Assert.That(reply.Message, Is.EqualTo("10000"));
        }

        [Test]
        public void Execute_ElementWithoutGridPattern_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<IGridPattern>>(() => Execute(clsQuery.Parse("uiatablerowcount")));
        }
    }
}