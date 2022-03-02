using System;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker.Sessions
{
    public class ServerProcessInfoLoader : IProcessInfoLoader
    {

        public ProcessInfo GetProcess(Guid processId)
        {
            var runMode = GetProcessRunMode(processId);
            var xml = GetProcessXml(processId);
            return new ProcessInfo(processId, runMode, xml);
        }

        private string GetProcessXml(Guid processId)
        {
            var processXml = "";
            var lastModified = new DateTime();
            var error = "";

            if (!clsAPC.ProcessLoader.GetProcessXML(processId, ref processXml, ref lastModified, ref error))
                throw new ArgumentException(error);

            return processXml;
        }

        private BusinessObjectRunMode GetProcessRunMode(Guid processId)
        {
            return clsAPC.ProcessLoader.GetEffectiveRunMode(processId);
        }
    }
}