using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.Server.Core;
using System;

namespace BluePrism.StartUp.Modules
{
    /// <summary>
    /// Registers IServer component in the container
    /// </summary>
    public class ServerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => app.gSv).As<IServer>().ExternallyOwned();

            //Server factory for components that are dependent on IServer but are resolved before gSv has been set up
            builder.Register<Func<IServer>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return () => context.Resolve<IServer>();
            });

            builder.RegisterType<SqlHelper>().As<ISqlHelper>();
        }
    }
}
