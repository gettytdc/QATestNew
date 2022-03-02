using System;
using InstructionalConnection;

namespace BluePrism.ClientServerResources.Grpc.Events
{
    internal class GetStatusResponseEventArgs
        : EventArgs
    {
        public GetStatusResponse Response { get; }

        public GetStatusResponseEventArgs(GetStatusResponse response)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }
    }
}
