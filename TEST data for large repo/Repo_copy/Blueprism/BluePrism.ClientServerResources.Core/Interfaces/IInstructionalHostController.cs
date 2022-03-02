using System;
using BluePrism.ClientServerResources.Core.Data;

namespace BluePrism.ClientServerResources.Core.Interfaces
{
    public interface IInstructionalHostController : IDisposable 
    {
        void ResourceStatusChanged(ResourcesChangedData data);
        void SessionCreated(SessionCreatedData data);
        void SessionDelete(SessionDeletedData data);
        void SessionEnd(SessionEndData data);
        void SessionStart(SessionStartedData data);
        void SessionStop(SessionStopData data);
        void SessionVariablesUpdated(SessionVariablesUpdatedData data);
    }
}
