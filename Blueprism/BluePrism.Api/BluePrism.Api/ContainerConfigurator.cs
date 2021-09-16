namespace BluePrism.Api
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Autofac.Builder;
    using Autofac.Extras.DynamicProxy;
    using Autofac.Integration.WebApi;
    using BpLibAdapters;
    using BpLibAdapters.Mappers.FilterMappers;
    using Common.Security;
    using Domain;
    using Filters;
    using FluentValidation;
    using Func;
    using Func.AspNet;
    using Logging;
    using Mappers.ErrorMappers;
    using Mappers.FilterMappers;
    using Services;

    using static LoggingConfiguration;

    public static class ContainerConfigurator
    {
        public static IContainer ConfigureContainer(ContainerBuilder builder)
        {
            BluePrismAssemblyLoader.LoadAssemblies(Assembly.GetExecutingAssembly().FullName);

            var scanAssemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.FullName.StartsWith("BluePrism.Api") || x.FullName.StartsWith("BluePrism.Logging"))
                    .ToArray();

            builder
                .RegisterAssemblyTypes(scanAssemblies)
                .AsImplementedInterfaces()
                .AddTraceLoggingIfEnabled();

            builder
                .RegisterAssemblyModules(scanAssemblies);

            RegisterLoggingServices(builder);
            RegisterValidators(builder, scanAssemblies);

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterInstance(
                    new ConnectionSettingProperties
                    {
                        ConnectionName = ConfigurationManager.AppSettings["ConnectionName"],
                        ServerName = ConfigurationManager.AppSettings["ServerName"],
                        DatabaseName = ConfigurationManager.AppSettings["DatabaseName"],
                        DatabaseUsername = ConfigurationManager.AppSettings["DatabaseUsername"],
                        DatabasePassword = ConfigurationManager.AppSettings["DatabasePassword"]?.AsSecureString(),
                        UsesWindowAuth = bool.Parse(ConfigurationManager.AppSettings["UsesWindowAuth"])
                    })
                .AsSelf();

            builder.RegisterInstance(
                    new BluePrismUserSettings
                    {
                        Username = ConfigurationManager.AppSettings["BPusername"],
                        Password = ConfigurationManager.AppSettings["BPpassword"].AsSecureString(),
                    })
                .AsSelf();

            builder.RegisterInstance(
                    new PagingSettings
                    {
                        MaxItemsPerPage = ConfigurationManager.AppSettings["MaxItemsPerPage"].Map(int.Parse)
                    })
                .AsSelf();

            builder.RegisterInstance(
                    new SessionLogConfiguration
                    {
                        MaxResultStringLength = ConfigurationManager.AppSettings["SessionLogs.MaxResultTextLength"].Map(int.Parse),
                    })
                .AsSelf();

            builder.RegisterInstance(
                    new CreateWorkQueueItemsSettings()
                    {
                        MaxCreateWorkQueueRequestsInBatch = ConfigurationManager.AppSettings["CreateWorkQueueItems.MaxRequestsInBatch"].Map(int.Parse),
                        MaxTagLength = ConfigurationManager.AppSettings["CreateWorkQueueItems.MaxTagLength"].Map(int.Parse),
                        MaxStatusLength = ConfigurationManager.AppSettings["CreateWorkQueueItems.MaxStatusLength"].Map(int.Parse)
                    })
                .AsSelf();
            builder.RegisterInstance(new CallTokenAccessor()).As<ITokenAccessor>();
            builder.Register(context =>
            {
                var authServerUrl = string.Empty;
                context.Resolve<IAuthService>().GetAuthServerUrl().Result.OnSuccess(url => authServerUrl = url);
                return new OpenIdConfiguration
                {
                    Authority = authServerUrl,
                    Audience = ConfigurationManager.AppSettings["Authorization.Audience"]
                };
            });
            builder.Register(context => new OpenIdConnectSigningKeyResolver(context.Resolve<OpenIdConfiguration>().Authority)).As<ISigningKeyResolver>().SingleInstance();
            RegisterFilterMappers(builder);
            RegisterErrorMappers(builder);
            RegisterAdapterGenerics(builder);

            builder.RegisterType<CommaDelimitedCollectionValidationFilterAttribute>().AsSelf();

            AdditionalConfiguration(builder);

            return builder.Build();
        }

        public static Func<ContainerBuilder, ContainerBuilder> AdditionalConfiguration { get; set; } = builder => builder;

        private static void RegisterLoggingServices(ContainerBuilder builder)
        {
            builder.RegisterType<LoggerFactory>().As<ILoggerFactory>().SingleInstance();

            builder.RegisterType<TraceLoggingAspect>().AsSelf();
            builder.RegisterGeneric(typeof(AsyncAspectWrapper<>));
            builder.RegisterType<TraceLoggingActionFilter>().AsSelf();
            builder.RegisterGeneric(typeof(NLogLoggerWrapper<>)).As(typeof(ILogger<>));
        }

        private static void RegisterValidators(ContainerBuilder builder, Assembly[] scanAssemblies)
        {
            var validatorInterfaceName = typeof(IValidator<>).Name;

            builder.RegisterAssemblyTypes(scanAssemblies)
                .Where(x => x.GetInterface(validatorInterfaceName) != null)
                .As(x => x.GetInterface(validatorInterfaceName));
        }

        private static void RegisterAdapterGenerics(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(AdapterStore<>)).AsImplementedInterfaces().SingleInstance();
            builder.RegisterGeneric(typeof(AdapterAuthenticatedMethodRunner<>)).AsImplementedInterfaces().SingleInstance();
            builder.RegisterGeneric(typeof(AdapterAnonymousMethodRunner<>)).AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ServerStore>().As<IServerStore>().SingleInstance();
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AddTraceLoggingIfEnabled<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> @this) =>
            TraceLoggingIsEnabled()
            ? @this.EnableInterfaceInterceptors().InterceptedBy(typeof(AsyncAspectWrapper<TraceLoggingAspect>))
            : @this;

        private static void RegisterFilterMappers(ContainerBuilder builder)
        {
            var filterModelMapperTypes =
                typeof(IFilterModelMapper)
                .Assembly
                .GetExportedTypes()
                .Where(x => x.IsClass && !x.IsAbstract)
                .Where(typeof(IFilterModelMapper).IsAssignableFrom)
                .ForEach(x => builder.RegisterType(x).AsSelf())
                .ToArray();

            builder.Register(ctx =>
                    filterModelMapperTypes
                        .Select(ctx.Resolve)
                        .OfType<IFilterModelMapper>()
                        .ToArray())
                .As<IReadOnlyCollection<IFilterModelMapper>>();

            var filterMapperTypes =
                typeof(IFilterMapper)
                .Assembly
                .GetExportedTypes()
                .Where(x => x.IsClass && !x.IsAbstract)
                .Where(typeof(IFilterMapper).IsAssignableFrom)
                .ForEach(x => builder.RegisterType(x).AsSelf())
                .ToArray();

            builder.Register(ctx =>
                    filterMapperTypes
                        .Select(ctx.Resolve)
                        .OfType<IFilterMapper>()
                        .ToArray())
                .As<IReadOnlyCollection<IFilterMapper>>();
        }

        private static void RegisterErrorMappers(ContainerBuilder builder)
        {
            IEnumerable<(Type Implementation, Type Interface, Type ErrorType)> GetErrorMapperTypes(Type type) =>
                type.GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .Where(x => x.GetGenericTypeDefinition() == typeof(IErrorMapper<>))
                    .Select(x => (type, x, x.GetGenericArguments().Single()));

            Func<ResultError, ResponseDetails, ErrorResponse> GetErrorMapperMethod((Type Implementation, Type Interface, Type ErrorType) mapperInfo, IComponentContext context)
            {
                var method = mapperInfo.Interface.GetMethod(nameof(IErrorMapper<ResultError>.GetResponseForError));
                var implementation = context.Resolve(mapperInfo.Interface);
                return (error, responseDetails) => method.Invoke(implementation, new object[] { error, responseDetails }) as ErrorResponse;
            }

            builder.RegisterType<ApiErrorResponseConverter>().As<IErrorResponseConverter>();

            var errorMapperTypes =
                typeof(IErrorMapper<>)
                    .Assembly
                    .GetExportedTypes()
                    .Where(x => x.IsClass && !x.IsAbstract)
                    .SelectMany(GetErrorMapperTypes)
                    .ToArray();

            errorMapperTypes.ForEach(x => builder.RegisterType(x.Implementation).As(x.Interface)).Evaluate();

            builder.Register(context =>
                {
                    var localContext = context.Resolve<IComponentContext>();
                    return errorMapperTypes.ToDictionary(
                        k => k.ErrorType,
                        v => GetErrorMapperMethod(v, localContext));
                })
                .As<IReadOnlyDictionary<Type, Func<ResultError, ResponseDetails, ErrorResponse>>>();

            builder.RegisterType<ApiExceptionResponseConverter>().As<IExceptionResponseConverter>().SingleInstance();
        }
    }
}
