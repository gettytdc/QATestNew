using System;
using BluePrism.ClientServerResources.Core.Events;

namespace BluePrism.ClientServerResources.Core.Interfaces
{
    public interface IInstructionalClientController
        : IDisposable
    {
        event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        event EventHandler<SessionCreateEventArgs> SessionCreated;
        event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        event EventHandler<SessionEndEventArgs> SessionEnd;
        event EventHandler<SessionStartEventArgs> SessionStarted;
        event EventHandler<SessionStopEventArgs> SessionStop;
        event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;
        event InvalidResponseEventHandler ErrorReceived;

        int TokenTimeoutInSeconds { get; set; }
        int ReconnectIntervalSeconds { get; set; }
        int KeepAliveTimeMS { get; set; }
        int KeepAliveTimeoutMS { get; set; }

        void EnsureConnected();
        void Connect();
        void Close();
        void DeRegister();
        void RegisterClient();
    }
}
