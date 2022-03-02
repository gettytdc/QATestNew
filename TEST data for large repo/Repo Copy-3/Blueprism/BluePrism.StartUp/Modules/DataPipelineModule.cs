
using Autofac;
using BluePrism.DataPipeline;

namespace BluePrism.StartUp.Modules
{
    public class DataPipelineModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JSONEventSerialiser>().As<IEventSerialiser>();
            builder.RegisterType<DataPipelinePublisher>().As<IDataPipelinePublisher>();
        }
    }
}
