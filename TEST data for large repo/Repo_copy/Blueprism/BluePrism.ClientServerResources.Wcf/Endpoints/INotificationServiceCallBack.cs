using System;
using System.ServiceModel;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Core.Events;

namespace BluePrism.ClientServerResources.Wcf.Endpoints
{
    [ServiceContract]
    public interface INotificationServiceCallBack
    {
        event EventHandler Message;
        event EventHandler<ResourcesChangedEventArgs> ResourceStatus;
        event EventHandler<SessionCreateEventArgs> SessionCreated;
        event EventHandler<SessionDeleteEventArgs> SessionDeleted;
        event EventHandler<SessionEndEventArgs> SessionEnd;
        event EventHandler<SessionStopEventArgs> SessionStop;
        event EventHandler<SessionStartEventArgs> SessionStarted;
        event EventHandler<SessionVariableUpdatedEventArgs> SessionVariableUpdated;

        [OperationContract(IsOneWay = true)]
        void OnMessage();
        [OperationContract(IsOneWay = true)]
        void OnResourceStatus(ResourcesChangedData resourcesChangedData);
        [OperationContract(IsOneWay = true)]
        void OnSessionCreated(SessionCreatedData sessionCreatedData);
        [OperationContract(IsOneWay = true)]
        void OnSessionDeleted(SessionDeletedData sessionDeletedData);
        [OperationContract(IsOneWay = true)]
        void OnSessionEnd(SessionEndData sessionEndData);
        [OperationContract(IsOneWay = true)]
        void OnSessionStarted(SessionStartedData sessionStartedData);
        [OperationContract(IsOneWay = true)]
        void OnSessionStop(SessionStopData sessionStopData);
        [OperationContract(IsOneWay = true)]
        void OnSessionVariableUpdated(SessionVariablesUpdatedData sessionVariablesUpdatedData);
    }
}
