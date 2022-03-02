using Autofac;
using MassTransit;

namespace BluePrism.AuthenticationServerSynchronization.Modules
{
    public class MessageBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<MessageBus>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return new MessageBus(
                        context.Resolve<IBusControl>());
            });
        }
    }
}
