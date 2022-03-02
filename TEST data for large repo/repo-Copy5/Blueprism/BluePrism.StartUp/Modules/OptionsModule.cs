using Autofac;
using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Config;

namespace BluePrism.StartUp.Modules
{
    public class OptionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => Options.Instance).As<IOptions>().ExternallyOwned();

            //Server factory for components that are dependent on IOptions but are resolved before Options has been set up
            builder.Register<Func<IOptions>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return () => context.Resolve<IOptions>();
            });
        }
    }
}