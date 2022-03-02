using BluePrism.BPCoreLib;

namespace BluePrism.ClientServerResources.Core.Enums
{
    public enum CallbackConnectionProtocol
    {
        None    = 0,
        [FriendlyName("gRPC")]
        Grpc    = 1,
        [FriendlyName("WCF")]
        Wcf     = 2
    }
}
