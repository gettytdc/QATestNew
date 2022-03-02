using System.Collections.Generic;

namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Interface for the event handler
    /// </summary>
    public interface IEventHandler
    {
        string Name { get; }
        void HandleEvent(IDictionary<string, object> data);
        IConfiguration Configuration { get; }
    }
}
