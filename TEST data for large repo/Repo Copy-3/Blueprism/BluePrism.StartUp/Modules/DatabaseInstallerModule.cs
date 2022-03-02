using Autofac;
using BluePrism.DatabaseInstaller;
using System;

namespace BluePrism.StartUp.Modules
{
    public class DatabaseInstallerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmbeddedResourceLoader>().As<IEmbeddedResourceLoader>();
            builder.RegisterType<DatabaseScriptLoader>().As<IDatabaseScriptLoader>();
            builder.RegisterType<SqlConnectionFactory>().As<ISqlConnectionFactory>();
            builder.RegisterType<DatabaseScriptGenerator>().As<IDatabaseScriptGenerator>();
            builder.RegisterType<SqlConnectionWrapper>().As<ISqlConnectionWrapper>();

            builder.Register<Func<ISqlDatabaseConnectionSetting, TimeSpan, IDatabase>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return (ISqlDatabaseConnectionSetting settings, TimeSpan timeout)
                    => new Database(
                        context.Resolve<ISqlConnectionFactory>(),
                        context.Resolve<IDatabaseScriptLoader>(),
                        settings,
                        timeout);
            });

            builder.Register<Func<ISqlDatabaseConnectionSetting, TimeSpan, string, string, IInstaller>>(ctx =>
            {
                var context = ctx.Resolve<IComponentContext>();
                return (ISqlDatabaseConnectionSetting settings, TimeSpan timeout, string appName, string ssoAuditCode)
                    => new DatabaseInstaller.Installer(
                        settings,
                        timeout,
                        appName,
                        ssoAuditCode,
                        context.Resolve<Func<ISqlDatabaseConnectionSetting, TimeSpan, IDatabase>>(),
                        context.Resolve<IDatabaseScriptGenerator>(),
                        context.Resolve<ISqlConnectionWrapper>(),
                        context.Resolve<IDatabaseScriptLoader>());
            });
        }
    }
}