using Autofac;
using BluePrism.Core.Utility;

namespace BluePrism.StartUp.Modules
{
    using AutomateAppCore;
    using AutomateProcessCore;
    using BluePrism.Core.ActiveDirectory.UserQuery;
    using BluePrism.Core.ActiveDirectory.DirectoryServices;
    using BluePrism.Core.Network;
    using Core.Configuration;
    using System;
    using System.DirectoryServices;
    using BluePrism.AutomateAppCore.clsServerPartialClasses.ActiveDirectory;
    using BluePrism.AutomateAppCore.Groups;

    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SystemClockWrapper>().As<ISystemClock>();
            builder.RegisterType<SystemTimerWrapper>().As<ISystemTimer>();
            builder.RegisterType<AppSettings>().As<IAppSettings>();
            builder.RegisterType<ObjectLoader>().As<IObjectLoader>();
            builder.RegisterType<DNSService>().As<IDNSService>();
            builder.RegisterType<NetworkInterfaces>().As<INetworkInterfaces>();
            builder.RegisterType<TcpClientFactory>().As<ITcpClientFactory>();
            builder.RegisterType<IPv6TcpClientFactory>().As<IIPv6TcpClientFactory>();
            builder.RegisterType<DirectorySearcherWrapper>().As<IDirectorySearcher>();
            builder.RegisterType<DirectorySearcherBuilder>().As<IDirectorySearcherBuilder>();
            builder.RegisterType<ActiveDirectoryUserQuery>().As<IActiveDirectoryUserQuery>();
            builder.RegisterType<DirectorySearcher>();
            builder.RegisterType<RetentiveGroupStore>().As<IGroupStore>().SingleInstance();
            builder.Register<Func<IDirectorySearcherBuilder>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return () => context.Resolve<IDirectorySearcherBuilder>();
            });
            builder.RegisterType<MappedActiveDirectoryUserFinder>().As<IMappedUserFinder>();            
        }
    }
}
