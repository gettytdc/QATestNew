using System;
using BluePrism.AutomateProcessCore;

namespace BluePrism.DigitalWorker
{
    public interface IProcessDataAccess
    {
        string GetProcessXml(Guid processId);
        BusinessObjectRunMode GetProcessRunMode(Guid processId);
    }
}