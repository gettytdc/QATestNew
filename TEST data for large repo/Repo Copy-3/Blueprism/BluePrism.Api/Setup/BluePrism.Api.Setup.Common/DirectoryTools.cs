namespace BluePrism.Api.Setup.Common
{
    using System.IO;

    public class DirectoryTools : IDirectoryTools
    {
        public string[] GetDirectories(string path) => Directory.GetDirectories(path);

        public string[] GetFiles(string path) => Directory.GetFiles(path);
    }
}
