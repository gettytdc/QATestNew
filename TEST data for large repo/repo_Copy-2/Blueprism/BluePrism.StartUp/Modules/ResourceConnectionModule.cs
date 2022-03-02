using System;
using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Resources;

namespace BluePrism.StartUp.Modules
{
    public class ResourceConnectionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RobotAddressStore>().As<IRobotAddressStore>().WithParameter("refreshPeriodSeconds", 120).SingleInstance();
            builder.Register<IUserAuthResourceConnectionManager>((c, p) => new OnDemandConnectionManager())
              .As<IUserAuthResourceConnectionManager>();
            builder.Register<IResourceConnectionManager>((c, p) =>
            {
                var connectionType = p.TypedAs<ConnectionType>();
                var appServerConnection = p.TypedAs<bool>();
                var context = c.Resolve<IComponentContext>();

                if (appServerConnection && connectionType == ConnectionType.BPServer)
                {
                    return new ServerConnectionManager(context.Resolve<IServer>());
                }

                return new PersistentConnectionManager(IResourceConnectionManager.Modes.Normal, null, Guid.Empty,
                    Guid.Empty);
            }).As<IResourceConnectionManager>();
        }
    }
}
