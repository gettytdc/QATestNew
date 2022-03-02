
namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Helper methods for interacting with Java processes.
    /// </summary>
    public interface IJavaProcessHelper
    {
        int GetProcessIdWthStartupParamsContaining(string textToFind);
    }
}
