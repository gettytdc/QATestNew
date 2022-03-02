namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Interface for the Factory method classes.
    /// </summary>
    public interface IEventHandlerFactory
    {
        string Name { get; }
        IEventHandler Create(string instanceName);
    }
}
