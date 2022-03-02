using System;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker
{
    public class ProcessDataAccess : IProcessDataAccess
    {
        public ProcessDataAccess()
        {
        }
        
        public string GetProcessXml(Guid processId)
        {
            var processXml = "";
            var lastModified = new DateTime();
            var error = "";

            if (!clsAPC.ProcessLoader.GetProcessXML(processId, ref processXml, ref lastModified, ref error))
                throw new InvalidOperationException(error);

            return processXml;
        }

        public BusinessObjectRunMode GetProcessRunMode(Guid processId)
        {
            return clsAPC.ProcessLoader.GetEffectiveRunMode(processId);
        }
    }
}
