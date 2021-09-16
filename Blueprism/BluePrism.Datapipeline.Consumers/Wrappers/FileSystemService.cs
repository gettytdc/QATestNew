using System.IO;

namespace BluePrism.Datapipeline.Logstash.Wrappers
{
    public class FileSystemService : IFileSystemService
    {
        public void CreateDirectory(string directory) => Directory.CreateDirectory(directory);


        public bool DirectoryExists(string directory) => Directory.Exists(directory);
        

        public void WriteToFile(string filePath, string text) => File.WriteAllText(filePath, text);
    }
}
