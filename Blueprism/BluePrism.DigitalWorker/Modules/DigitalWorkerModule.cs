using System;
using Autofac;
using BluePrism.AutomateAppCore.Resources;
using BluePrism.AutomateProcessCore;
using BluePrism.Core.Utility;
using BluePrism.DigitalWorker.EnvironmentFunctions;
using BluePrism.DigitalWorker.Messaging;
using BluePrism.DigitalWorker.Messaging.Observers;
using BluePrism.DigitalWorker.Notifications;
using BluePrism.DigitalWorker.Sessions;
using BluePrism.DigitalWorker.Sessions.Coordination;

namespace BluePrism.DigitalWorker.Modules
{
    public class DigitalWorkerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(ctx => DigitalWorkerContextStore.Current).ExternallyOwned();
            builder.RegisterType<NotificationHandler>().As<INotificationHandler>().SingleInstance();
            builder.RegisterType<ProcessDataAccess>().As<IProcessDataAccess>().SingleInstance();
            builder.RegisterType<SessionCoordinator>().As<ISessionCoordinator>().SingleInstance();
            builder.RegisterType<ServerProcessInfoLoader>().As<IProcessInfoLoader>();
            builder.RegisterType<RunProcessQueueCoordinator>().As<IRunProcessQueueCoordinator>();
            builder.RegisterType<LifecycleEventPublisher>().AsImplementedInterfaces();
            builder.RegisterType<SessionServiceClient>().AsImplementedInterfaces();
            builder.RegisterType<ExclusiveProcessLock>().As<IExclusiveProcessLock>().SingleInstance();
            builder.RegisterType<ExclusiveProcessLockObserver>().As<IExclusiveProcessLockObserver>();
            builder.RegisterType<RunningSessionMonitor>();
            builder.RegisterType<SessionRunner>().As<ISessionRunner>();
            builder.RegisterType<LogEventPublisher>().As<ILogEventPublisher>();
            builder.RegisterType<SessionStatusPublisher>().As<ISessionStatusPublisher>();
            builder.RegisterType<SessionCleanup>().As<ISessionCleanup>();
            builder.RegisterType<MessageBusWrapper>().As<IMessageBusWrapper>();
            builder.RegisterType<TaskDelay>().As<ITaskDelay>();

            builder.Register<Func<Guid, IProcessLogContext, DigitalWorkerSessionLoggingEngine>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return (sessionId, logContext)
                    => new DigitalWorkerSessionLoggingEngine(
                        context.Resolve<ILogEventPublisher>(),
                        logContext, 
                        sessionId);
            });

            builder.Register<Func<SessionContext, ISessionStatusPublisher>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return sessionContext
                    => new SessionStatusPublisher(
                        context.Resolve<IMessageBusWrapper>(),
                        context.Resolve<ISystemClock>(),
                        sessionContext);
            });

            builder.Register<Func<SessionContext, IDigitalWorkerRunnerRecord>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return (sessionContext)
                    => new DigitalWorkerRunnerRecord(
                        sessionContext,
                        context.Resolve<RunningSessionMonitor>(),
                        context.Resolve<Func<SessionContext, ISessionStatusPublisher>>()(sessionContext),
                        context.Resolve<ISessionCleanup>(),
                        context.Resolve<Func<Guid, IProcessLogContext, DigitalWorkerSessionLoggingEngine>>()
                        );
            });

            builder.Register<Func<IResourcePCView, DigitalWorker>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return view
                    => new DigitalWorker(
                        context.Resolve<IMessageBusWrapper>(),
                        context.Resolve<DigitalWorkerContext>(),
                        view,
                        context.Resolve<ISessionServiceClient>(),
                        context.Resolve<IRunProcessQueueCoordinator>(),
                        context.Resolve<INotificationHandler>(),
                        context.Resolve<ILifecycleEventPublisher>(),
                        context.Resolve<ITaskDelay>());
            });

            builder.RegisterType<RunningSessionRegistry>().As<IRunningSessionRegistry >().SingleInstance();

            builder.RegisterType<IsStopRequestedFunction>();
            builder.RegisterType<GetStartTimeFunction>();
            builder.RegisterType<GetUserNameFunction>();
            builder.RegisterType<GetResourceNameFunction>();

            builder.Register<Func<DigitalWorkerContext>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return () => context.Resolve<DigitalWorkerContext>();
            });
        }
    }
}
