using BluePrism.Datapipeline.Logstash.Wrappers;
using System.Diagnostics;

namespace BluePrism.Datapipeline.Logstash
{
    public class ProcessFactory : IProcessFactory
    {
        public IProcess CreateProcess()
        {
            return new ProcessWrapper(new Process());
        }

        public IProcess GetProcessById(int id)
        {
            return new ProcessWrapper(Process.GetProcessById(id));
        }
    }
}
