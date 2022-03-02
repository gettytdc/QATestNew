using BluePrism.ClientServerResources.Grpc.Interfaces;
using Grpc.Core;

namespace BluePrism.ClientServerResources.Grpc.Services
{
    public class GrpcServiceFactory : IGrpcServiceFactory
    {
        public Server CreateServer() => new Server();
    }
}
