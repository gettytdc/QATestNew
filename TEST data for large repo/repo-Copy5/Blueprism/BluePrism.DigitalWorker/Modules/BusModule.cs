using System;
using Autofac;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Config;
using BluePrism.Common.Security;
using BluePrism.Cirrus.Sessions.SessionService.Messages.Commands;
using BluePrism.Cirrus.Sessions.SessionService.MessagingClient;
using MassTransit;
using MassTransit.RabbitMqTransport;
using NLog.Extensions.Logging;
using LogContext = MassTransit.Context.LogContext;

namespace BluePrism.DigitalWorker.Modules
{
    public class BusModule : Module
    {
        private static readonly RabbitMqConfiguration DefaultRabbitMqConfiguration = new RabbitMqConfiguration("rabbitmq://localhost/", "guest", "guest".AsSecureString());

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddMassTransit(config =>
            {
                config.AddConsumers(ThisAssembly);
                config.UsingRabbitMq(BusFactory);
                config.AddRequestClient<RegisterDigitalWorker>(new Uri($"queue:{SessionServiceQueues.RegisterDigitalWorker}"));
                config.AddRequestClient<RequestStartProcess>(new Uri($"queue:{SessionServiceQueues.RequestStartProcess}"));
            });

        }

        private static void BusFactory(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
        {
            LogContext.ConfigureCurrentLogContext(new NLogLoggerFactory());

            var configuration = context.GetService<IOptions>().DbConnectionSetting.RabbitMqConfiguration ?? DefaultRabbitMqConfiguration;

            configurator.Host(configuration.HostUrl, h =>
            {
                h.Username(configuration.Username);
                h.Password(configuration.Password.AsString());
            });
        }
    }
}
