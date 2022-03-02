using System;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker.Sessions
{
    public class ProcessInfo
    {
        public ProcessInfo(Guid processId, BusinessObjectRunMode effectiveRunMode, string processXml)
        {
            ProcessId = processId;
            EffectiveRunMode = effectiveRunMode;
            ProcessXml = processXml;
        }
        
        public Guid ProcessId { get; }

        public BusinessObjectRunMode EffectiveRunMode { get; }

        public string ProcessXml { get; }

        public override string ToString()
        {
            return $"{nameof(ProcessId)}: {ProcessId}, {nameof(EffectiveRunMode)}: {EffectiveRunMode}";
        }
    }
}