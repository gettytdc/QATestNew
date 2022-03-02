namespace BluePrism.Core.Configuration
{
    public interface IAppSettings
    {
        string this[string key] { get; }
    }
}