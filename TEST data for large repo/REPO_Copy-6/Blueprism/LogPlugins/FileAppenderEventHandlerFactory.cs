using System.ComponentModel.Composition;

namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Factory for the file appender event handler
    /// </summary>
    [Export(typeof(IEventHandlerFactory))]
    public class FileAppenderEventHandlerFactory : IEventHandlerFactory
    {
        public string Name
        {
            get { return "File Appender"; }
        }

        public IEventHandler Create(string instanceName)
        {
            return new FileAppenderEventHandler() { Name = instanceName };
        }
    }
}
