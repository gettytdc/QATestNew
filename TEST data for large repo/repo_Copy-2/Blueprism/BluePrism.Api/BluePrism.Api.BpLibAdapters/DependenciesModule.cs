namespace BluePrism.Api.BpLibAdapters
{
    using Autofac;
    using AutomateAppCore;
    using StartUp;

    public class DependenciesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BluePrismServerFactory>().AsSelf().SingleInstance();

            builder.Register(_ =>
            {
                Options.Instance.Init(ConfigLocator.Instance());
                return Options.Instance;
            }).SingleInstance();

            builder.RegisterType<DatabaseBackedScheduleStore>().As<Scheduling.IScheduleStore>().SingleInstance();

            ContainerInitialiser.SetUpContainer();
        }
    }
}
