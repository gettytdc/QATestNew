using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared;
using BluePrism.ApplicationManager.CommandHandling;
using BluePrism.UIAutomation;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{
    internal class UIAutomationHandlerBaseTests
    {
        [Test]
        public void Execute_WithQuery_ShouldCallExecuteWithElement()
        {
            var applicationMock = new Mock<ILocalTargetApp>();
            int pid = 123;
            applicationMock.SetupGet(a => a.PID).Returns(pid);
            var query = clsQuery.Parse("UIATest");
            var elementMock = new Mock<IAutomationElement>();
            var identifierHelperMock = new Mock<IUIAutomationIdentifierHelper>();
            identifierHelperMock.Setup(i => i.FindUIAutomationElement(query, pid)).Returns(elementMock.Object);
            var handler = new TestHandler(applicationMock.Object, identifierHelperMock.Object);
            handler.Execute(new CommandContext(query));
            Assert.That(handler.ElementUsed, Is.EqualTo(elementMock.Object));
            Assert.That(handler.ContextUsed.Query, Is.EqualTo(query));
        }

        internal class TestHandler : UIAutomationHandlerBase
        {
            public TestHandler(ILocalTargetApp application, IUIAutomationIdentifierHelper identifierHelper) : base(application, identifierHelper)
            {
            }

            protected override Reply Execute(IAutomationElement element, CommandContext context)
            {
                ElementUsed = element;
                ContextUsed = context;
                return Reply.Ok;
            }

            public CommandContext ContextUsed { get; set; }
            public IAutomationElement ElementUsed { get; set; }
        }
    }
}