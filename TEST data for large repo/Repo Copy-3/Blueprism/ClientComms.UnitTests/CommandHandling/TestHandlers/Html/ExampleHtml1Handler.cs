using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.Html
{
    [CommandId("HtmlExample1")]
    internal class ExampleHtml1Handler : HtmlHandlerBase
    {
        public override Reply Execute(CommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}