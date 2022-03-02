using System;
using System.Threading.Tasks;
using BluePrism.ClientServerResources.Core.Data;
using BluePrism.ClientServerResources.Grpc.Events;
using Grpc.Core;
using InstructionalConnection;

namespace BluePrism.ClientServerResources.Grpc.Interfaces
{
    public interface IInstructionalConnectionService : IDisposable
    {
        event EventHandler<ClientRegisteredEventArgs> ClientRegistered;
        event EventHandler<ClientRegisteredEventArgs> ClientDeregistered;
        Task RegisterClient(IAsyncStreamReader<RegisterClientRequest> requestStream, IServerStreamWriter<RegisterClientResponse> responseStream, ServerCallContext context);
        Task<DeRegisterClientResponse> DeRegister(DeRegisterClientRequest request, ServerCallContext context);
        void EnqueueMessage(ResourcesChangedData data);
        void EnqueueMessage(SessionCreatedData data);
        void EnqueueMessage(SessionStopData data);
        void EnqueueMessage(SessionDeletedData data);
        void EnqueueMessage(SessionEndData data);
        void EnqueueMessage(SessionStartedData data);
        void EnqueueMessage(SessionVariablesUpdatedData data);
    }
}
