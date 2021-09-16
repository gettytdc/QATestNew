using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.UIAutomation
{
    [CommandId("UiaExample2")]
    public class ExampleUIAutomation2Handler : ICommandHandler
    {
        public Reply Execute(CommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}