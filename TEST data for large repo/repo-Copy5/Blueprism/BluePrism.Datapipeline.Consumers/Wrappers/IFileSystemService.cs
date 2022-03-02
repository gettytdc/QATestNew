namespace BluePrism.Datapipeline.Logstash.Wrappers
{
    public interface IFileSystemService
    {
        bool DirectoryExists(string directory);
        void CreateDirectory(string directory);

        void WriteToFile(string filePath, string text);
    }
}
