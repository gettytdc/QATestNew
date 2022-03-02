namespace BluePrism.Api.Setup.Common
{
    public interface IDirectoryTools
    {
        string[] GetDirectories(string path);
        string[] GetFiles(string path);
    }
}
