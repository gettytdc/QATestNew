using System;
using System.Data.SqlClient;
using Autofac;
using BluePrism.AutomateAppCore.Config;
using BluePrism.Cirrus.Common.MassTransit;
using BluePrism.Common.Security;
using BluePrism.AuthenticationServerSynchronization.Consumers;
using BluePrism.Server.Domain.Models;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using NLog.Extensions.Logging;
using LogContext = MassTransit.Context.LogContext;

namespace BluePrism.AuthenticationServerSynchronization.Modules
{
    public class MassTransitModule : Module
    {
        private static readonly string ServiceName = "BluePrismAppServer";
        private static readonly QueueNameBuilder QueueBuilder = new QueueNameBuilder(ServiceName);
        private static readonly ushort PrefetchCount = 1;
        private static readonly int ExceptionRetryCount = 5;
        private static readonly TimeSpan ExceptionInterval = TimeSpan.FromSeconds(5);

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddMassTransit(config =>
            {
                config.AddConsumers(ThisAssembly);
                config.UsingRabbitMq(BusFactory);
            });
        }

        private static void BusFactory(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            LogContext.ConfigureCurrentLogContext(new NLogLoggerFactory());

            var options = context.GetService<IOptions>();
            var brokerConfig = options.GetServerConfig(options.CurrentServerConfigName)?.AuthenticationServerBrokerConfig;

            if (brokerConfig == null)
            {
                throw new InvalidOperationException("Authentication server broker config is required");
            }

            configurator.Host(brokerConfig.BrokerAddress, h =>
            {
                h.Username(brokerConfig.BrokerUsername);
                h.Password(brokerConfig.BrokerPassword.AsString());
            });

            configurator.ReceiveEndpoint(QueueBuilder.Build("UserSynchronization", brokerConfig.EnvironmentIdentifier), e =>
            {
                e.PrefetchCount = PrefetchCount;

                e.ConfigureConsumer<UserCreatedConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                   {
                       cfg.Handle<SqlException>();
                       cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                   });
                });

                e.ConfigureConsumer<UserRetiredConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                    {
                        cfg.Handle<SqlException>();
                        cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                    });
                });
                
                e.ConfigureConsumer<ServiceAccountUpdatedConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                    {
                        cfg.Handle<SqlException>();
                        cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                    });
                });

                e.ConfigureConsumer<UserUnretiredConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                    {
                        cfg.Handle<SqlException>();
                        cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                    });
                });

                e.ConfigureConsumer<ServiceAccountCreatedConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                    {
                        cfg.Handle<SqlException>();
                        cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                    });
                });

                e.ConfigureConsumer<ServiceAccountDeletedConsumer>(context, configure =>
                {
                    configure.UseMessageRetry(cfg =>
                    {
                        cfg.Handle<SqlException>();
                        cfg.Interval(ExceptionRetryCount, ExceptionInterval);
                    });
                });
            });
        }
    }
}
