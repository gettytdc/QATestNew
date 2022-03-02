using Autofac;
using BluePrism.ActiveDirectoryUserSearcher.Services;

namespace BluePrism.StartUp.Modules
{
    public class ActiveDirectoryUserSearcherModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ActiveDirectoryUserSearchService>().As<IActiveDirectoryUserSearchService>();
        }
    }
}
