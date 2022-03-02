namespace BluePrism.StartUp.Modules
{
    using Autofac;
    using AutomateAppCore.clsServerPartialClasses.Caching;
    using Caching;

    public class CachingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterGeneric(typeof(InMemoryCacheWithDatabaseRefresh<>))
                .As(typeof(IRefreshCache<,>));

            builder
                .RegisterGeneric(typeof(InMemoryCache<>))
                .As(typeof(ICache<,>));

            builder
                .RegisterType<CacheDataProvider>()
                .As<ICacheDataProvider>();

            builder
                .RegisterType<CacheFactory>().As<ICacheFactory>().SingleInstance();
        }

    }
}