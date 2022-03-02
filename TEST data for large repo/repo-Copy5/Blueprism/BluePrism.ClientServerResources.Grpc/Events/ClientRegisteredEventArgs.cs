using System;

namespace BluePrism.ClientServerResources.Grpc.Events
{
    public class ClientRegisteredEventArgs : EventArgs
    {
        public Guid ClientId { get; private set; }
        public ClientRegisteredEventArgs(Guid clientId)
        {
            if(clientId == Guid.Empty )
            {
                throw new ArgumentException("ClientId has not been set", nameof(clientId));
            }
            ClientId = clientId;
        }
    }
}
