using Autofac;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Interfaces;
using BluePrism.ClientServerResources.Grpc.Interfaces;
using BluePrism.ClientServerResources.Grpc.Services;

namespace BluePrism.ClientServerResources.Grpc.Modules
{
    public class GrpcModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InstructionalClientGrpcController>().Keyed<IInstructionalClientController>(CallbackConnectionProtocol.Grpc);
            builder.RegisterType<InstructionalConnectionGrpcHost>().Keyed<IInstructionalHostController>(CallbackConnectionProtocol.Grpc);
            builder.RegisterType<InstructionalConnectionServiceImpl>().As<IInstructionalConnectionService>();
            builder.RegisterType<GrpcServiceFactory>().As<IGrpcServiceFactory>();
        }
    }
}
