using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandling;

namespace ClientComms.UnitTests.ClientComms.UnitTests.CommandHandling.TestHandlers.Html
{
    public abstract class HtmlHandlerBase : ICommandHandler
    {
        public abstract Reply Execute(CommandContext context);
    }
}