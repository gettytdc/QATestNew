using Grpc.Core;

namespace BluePrism.ClientServerResources.Grpc.Interfaces
{
    public interface IGrpcServiceFactory
    {
        Server CreateServer();
    }
}
