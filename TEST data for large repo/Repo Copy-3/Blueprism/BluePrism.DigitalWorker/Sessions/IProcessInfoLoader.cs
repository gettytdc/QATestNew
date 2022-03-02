using System;

namespace BluePrism.DigitalWorker.Sessions
{
    public interface IProcessInfoLoader
    {
        ProcessInfo GetProcess(Guid processId);
    }
}