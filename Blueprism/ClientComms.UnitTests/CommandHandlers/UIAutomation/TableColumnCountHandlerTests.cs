using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableColumnCountHandlerTests : UIAutomationHandlerTestBase<TableColumnCountHandler>
    {
        [Test]
        public void Execute_ElementWithGridPattern_ShouldReturnValue()
        {
            var patternMock = ElementMock.MockPattern<IGridPattern>();
            patternMock.Setup(p => p.CurrentColumnCount).Returns(10000);
            var reply = Execute(clsQuery.Parse("uiatablecolumncount"));
            Assert.That(reply.Message, Is.EqualTo("10000"));
        }
    }
}