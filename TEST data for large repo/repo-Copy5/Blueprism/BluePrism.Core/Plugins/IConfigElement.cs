namespace BluePrism.Core.Plugins
{
    /// <summary>
    /// Interface for the configuration element
    /// basically just a name/value pair.
    /// </summary>
    public interface IConfigElement
    {
        string Name { get; }
        object Value { get; set; }
    }
}
