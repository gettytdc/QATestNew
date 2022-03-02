namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BPCoreLib.DependencyInjection;
    using BpLibAdapters;
    using BpLibAdapters.Extensions;
    using Data;
    using FluentAssertions;
    using UnitTesting.TestSupport;

    public class MockBluePrismServerFactory : IBluePrismServerFactory
    {
        private readonly IDbConnection _databaseConnection;
        private readonly ConnectionSettingProperties _connectionSettingProperties;

        public MockBluePrismServerFactory(IDbConnection databaseConnection, ConnectionSettingProperties connectionSettingProperties)
        {
            _databaseConnection = databaseConnection;
            _connectionSettingProperties = connectionSettingProperties;
        }

        public IServer ClientInit()
        {
            var serverConstructor = typeof(clsServer).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(clsDBConnectionSetting), typeof(Dictionary<string, clsEncryptionScheme>) },
                null);

            serverConstructor.Should().NotBeNull();

            var server = (IServer)serverConstructor?.Invoke(new object[] { _connectionSettingProperties.ToDbConnectionSetting(), null });

            ReflectionHelper.SetPrivateField(typeof(clsServer), "mDatabaseConnectionFactory", server, (Func<IDatabaseConnection>)(() => new MockDatabaseConnection(_databaseConnection)));

            ReflectionHelper.SetPrivateField(typeof(ServerFactory), "mServerManager", null, new MockServerManager(server));

            //clsServer creates a scoped resolver which doesn't work well with mocks. To get around this we inject a mocked resolver that just points back to the default resolver
            ReflectionHelper.SetPrivateField(typeof(clsServer), "mDependencyResolver", server, new ResolverMock());

            ReflectionHelper.InvokePrivateMethod<Permission>("Init", null, new object[] {server});

            return server;
        }

        private class ResolverMock : IDependencyResolver
        {
            public T Resolve<T>() => DependencyResolver.Resolve<T>();

            public void Dispose()
            {
            }

        }
    }
}
