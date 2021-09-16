using System.Reflection;
using Autofac;
using Autofac.Extras.NLog;
using BluePrism.AuthenticationServerSynchronization.Modules;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.DependencyInjection;
using BluePrism.ClientServerResources.Grpc.Modules;
using BluePrism.ClientServerResources.Wcf.Modules;
using BluePrism.DigitalWorker.Modules;

namespace BluePrism.StartUp
{
    /// <summary>
    /// Initialises the application's shared dependency injection container
    /// </summary>
    public static class ContainerInitialiser
    {
        /// <summary>
        /// Sets up a shared dependency injection container. A new container
        /// is created and the required dependencies are registered within it 
        /// (Autofac modules within the BluePrism.StartUp assembly are responsible
        /// for registering the dependencies used throughout the application). The
        /// DependencyResolver class is then initialised with the new container so
        /// that it can be accessed where needed within the application.
        /// </summary>
        public static void SetUpContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            builder.RegisterModule<NLogModule>();
            builder.RegisterModule<BusModule>();
            builder.RegisterModule<MassTransitModule>();
            builder.RegisterModule<DigitalWorkerModule>();
            builder.RegisterModule<MessageBusModule>();
            builder.RegisterModule<WcfModule>();
            builder.RegisterModule<GrpcModule>();
            builder.RegisterModule<AutomateAppCoreModule>();
            var container = builder.Build();
            DependencyResolver.Initialise(container);
        }
    }
}
