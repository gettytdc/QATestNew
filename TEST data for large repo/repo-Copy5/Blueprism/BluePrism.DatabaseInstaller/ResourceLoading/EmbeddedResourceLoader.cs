using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BluePrism.DatabaseInstaller
{
    public class EmbeddedResourceLoader : IEmbeddedResourceLoader
    {
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        public IEnumerable<string> GetResourceNames()
            => _assembly.GetManifestResourceNames();

        public string GetResourceContent(string resourceName)
        {
            using(var stream = _assembly.GetManifestResourceStream(resourceName))
            {
                if (stream is null) return null;

                using(var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
