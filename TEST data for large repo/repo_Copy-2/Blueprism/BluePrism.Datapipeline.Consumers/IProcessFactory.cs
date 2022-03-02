

using BluePrism.Datapipeline.Logstash.Wrappers;

namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// Interface for creating / retreiving processes.
    /// </summary>
    public interface IProcessFactory
    {
        /// <summary>
        /// Creates a new Process.
        /// </summary>
        /// <returns></returns>
        IProcess CreateProcess();

        /// <summary>
        /// Returns the process with the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IProcess GetProcessById(int id);
    }
}
