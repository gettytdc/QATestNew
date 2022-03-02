using Autofac;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Interfaces;

namespace BluePrism.ClientServerResources.Wcf.Modules
{
    public class WcfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InstructionalConnectionWCFHost>().Keyed<IInstructionalHostController>(CallbackConnectionProtocol.Wcf);
            builder.RegisterType<InstructionalClientWCFController>().Keyed<IInstructionalClientController>(CallbackConnectionProtocol.Wcf);
        }
    }
}
