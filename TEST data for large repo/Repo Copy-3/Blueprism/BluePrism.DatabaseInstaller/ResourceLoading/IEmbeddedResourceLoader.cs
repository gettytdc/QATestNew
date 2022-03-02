using System.Collections.Generic;

namespace BluePrism.DatabaseInstaller
{
    public interface IEmbeddedResourceLoader
    {
        string GetResourceContent(string resourceName);
        IEnumerable<string> GetResourceNames();
    }
}
