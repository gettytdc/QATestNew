using System;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.Html
{
    [CommandId("HtmlExample2")]
    internal class ExampleHtml2Handler : HtmlHandlerBase
    {
        public override Reply Execute(CommandContext context)
        {
            throw new NotImplementedException();
        }
    }
}