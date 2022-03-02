using Autofac;
using BluePrism.AutomateAppCore.EnvironmentFunctions;
using System;
using BluePrism.Core.Utility;

namespace BluePrism.StartUp.Modules
{
    public class EnvironmentFunctionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return isDigitalWorker => isDigitalWorker ? context.Resolve<DigitalWorker.EnvironmentFunctions.IsStopRequestedFunction>() as EnvironmentFunction :
                                                            context.Resolve<IsStopRequestedFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return isDigitalWorker => isDigitalWorker ? context.Resolve<DigitalWorker.EnvironmentFunctions.GetStartTimeFunction>() as EnvironmentFunction :
                                                            context.Resolve<GetStartTimeFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return isDigitalWorker => isDigitalWorker ? context.Resolve<DigitalWorker.EnvironmentFunctions.GetResourceNameFunction>() as EnvironmentFunction :
                                                            context.Resolve<GetResourceNameFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                return isDigitalWorker => isDigitalWorker ? context.Resolve<DigitalWorker.EnvironmentFunctions.GetUserNameFunction>() as EnvironmentFunction :
                                                            context.Resolve<GetUserNameFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<GetSessionIdFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<GetBPVersionMajorFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<GetBPVersionMinorFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<IsBPServerFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<IsSingleSignOnFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<GetConnectionNameFunction>() as EnvironmentFunction;
            });

            builder.Register<Func<bool, EnvironmentFunction>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();

                // Returns the same function for both Digital Worker and Runtime Resource
                return isDigitalWorker => context.Resolve<GetClipboardFunction>() as EnvironmentFunction;
            });

            builder.RegisterType<ClipboardWrapper>().As<IClipboard>();
            builder.RegisterType<IsStopRequestedFunction>();
            builder.RegisterType<GetStartTimeFunction>();
            builder.RegisterType<GetResourceNameFunction>();
            builder.RegisterType<GetUserNameFunction>();
            builder.RegisterType<GetSessionIdFunction>();
            builder.RegisterType<GetBPVersionMajorFunction>();
            builder.RegisterType<GetBPVersionMinorFunction>();
            builder.RegisterType<IsBPServerFunction>();
            builder.RegisterType<IsSingleSignOnFunction>();
            builder.RegisterType<GetConnectionNameFunction>();
            builder.RegisterType<GetClipboardFunction>();
        }
    }
}
