using System;
using System.ServiceModel;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Wcf.Data;

namespace BluePrism.ClientServerResources.Wcf.Endpoints
{
    [ServiceContract(CallbackContract = typeof(INotificationServiceCallBack))]
    public interface INotificationServices
    {
        [OperationContract]
        Response RegisterClient(Guid id,string token);
        [OperationContract]
        Response UnRegisterClient(Guid id);

        [OperationContract]
        Response GetStatus(Guid id);

        void ResourceStatus(Guid id, ResourcesChangedData data);
        void SessionCreated(Guid id, SessionCreatedData data);
        void DeleteSession(Guid id, SessionDeletedData data);
        void SessionEnd(Guid id, SessionEndData data);
        void SessionStarted(Guid id, SessionStartedData data);
    }
}
