using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.UIAutomation
{
    [CommandId("UiaExample1")]
    internal class ExampleUIAutomation1Handler : ICommandHandler
    {
        public Reply Execute(CommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}