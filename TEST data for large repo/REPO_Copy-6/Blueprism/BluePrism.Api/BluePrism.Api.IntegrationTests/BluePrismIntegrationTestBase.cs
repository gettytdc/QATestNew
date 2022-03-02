namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Apps72.Dev.Data.DbMocker;
    using Autofac;
    using AutomateAppCore;
    using BPCoreLib.DependencyInjection;
    using BpLibAdapters;
    using BpLibAdapters.Mappers.FilterMappers;
    using Caching;
    using Common.Security;
    using DatabaseInstaller;
    using Domain;
    using Func;
    using Moq;
    using Services;
    using UnitTesting.TestSupport;
    using Utilities.Testing;

    public abstract class BluePrismIntegrationTestBase<TSubject> : IntegrationTestBase
    {
        protected const string TestUsername = "TestUser";
        protected const string TestPassword = "TestPassword";

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly Guid TestUserId = new Guid("00000000-0000-0000-0000-000000000001");

        protected TSubject Subject { get; private set; }
        protected MockDbConnection DatabaseConnection { get; private set; }

        public override void Setup(Action<ContainerBuilder> configure)
        {
            DatabaseConnection = new MockDbConnection();
            ConfigureDatabaseMocks();

            base.Setup(builder =>
            {
                builder.Register(context => new MockBluePrismServerFactory(DatabaseConnection, context.Resolve<ConnectionSettingProperties>()))
                    .As<IBluePrismServerFactory>()
                    .SingleInstance();

                builder.Register(context => context.Resolve<IBluePrismServerFactory>().ClientInit())
                    .SingleInstance();

                builder.RegisterType<UserStaticMethodWrapper>().As<IUserStaticMethodWrapper>().SingleInstance();
                builder.RegisterGeneric(typeof(AdapterStore<>)).AsImplementedInterfaces().SingleInstance();
                builder.RegisterType<ServerStore>().As<IServerStore>().SingleInstance();
                builder.RegisterGeneric(typeof(AdapterAuthenticatedMethodRunner<>)).AsImplementedInterfaces().SingleInstance();
                builder.RegisterGeneric(typeof(AdapterAnonymousMethodRunner<>)).AsImplementedInterfaces().SingleInstance();

                builder.RegisterAssemblyTypes(typeof(IServerAdapter).Assembly)
                    .Where(x => typeof(IServerAdapter).IsAssignableFrom(x))
                    .AsImplementedInterfaces();

                builder.RegisterInstance(new BluePrismUserSettings
                {
                    Username = TestUsername,
                    Password = TestPassword.AsSecureString(),
                });

                builder.RegisterInstance(new ConnectionSettingProperties
                {
                    ConnectionName = "!!!IntegrationTest!!!",
                    DatabaseName = "!!!INVALID!!!",
                    ServerName = "0.0.0.0",
                    UsesWindowAuth = true,
                });

                builder.RegisterInstance<Func<ISqlDatabaseConnectionSetting, TimeSpan, string, string, IInstaller>>(
                    (connectionSetting, time, username, password) =>
                        GetMock<IInstaller>().Object);

                builder.RegisterInstance<Func<string, IDbCommand>>(sql => new SqlCommand(sql));

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

                configure(builder);
            });

            GetMock<ICacheFactory>()
                .Setup(m => m.GetInMemoryCacheWithDatabaseRefresh<IGroupPermissions>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => GetMock<IRefreshCache<string, IGroupPermissions>>().Object);

            GetMock<ICacheFactory>()
                .Setup(m => m.GetInMemoryCacheWithDatabaseRefresh<List<Guid>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() => GetMock<IRefreshCache<string, List<Guid>>>().Object);

            GetMock<IRefreshCache<string, IGroupPermissions>>()
                .Setup(m => m.GetValue(It.IsAny<string>(), It.IsAny<Func<IGroupPermissions>>()))
                .Returns((string key, Func<IGroupPermissions> factory) => factory());

            GetMock<IRefreshCache<string, List<Guid>>>()
                .Setup(m => m.GetValue(It.IsAny<string>(), It.IsAny<Func<List<Guid>>>()))
                .Returns((string key, Func<List<Guid>> factory) => factory());

            GetMock<ITokenAccessor>()
                .SetupGet(m => m.TokenString)
                .Returns("testToken");

            ReflectionHelper.SetPrivateField(typeof(Options), "mMachineConfig", Options.Instance, new MachineConfig(GetMock<IConfigLocator>().Object));

            app.gAuditingEnabled = true;

            DependencyResolver.SetContainer(Container);
            Subject = Create<TSubject>();

            FilterMapper.SetFilterMappers(Container.Resolve<IReadOnlyCollection<IFilterMapper>>());
        }

        public override void Setup() => Setup(_ => { });

        protected void ConfigureFallbackForUpdateAndInsert() =>
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && (cmd.CommandText.Trim().StartsWith("update ", StringComparison.OrdinalIgnoreCase) ||
                        cmd.CommandText.Trim().StartsWith("insert ", StringComparison.OrdinalIgnoreCase)))
                .ReturnsScalar(1);

        private void ConfigureDatabaseMocks()
        {
            ConfigureUserAccountMocks();
            ConfigureGroupMocks();
            ConfigurePermissionMocks();

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("select dataname, versionno from BPADataTracker", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable.WithColumns("dataname", "versionno"));
        }

        private void ConfigureUserAccountMocks()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.Parameters.Count() == 1
                    && cmd.CommandText.Trim().Equals("select u.userid from BPAUser u where u.authenticationServerUserId = @authServerUserId", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable.SingleCell("userid", TestUserId));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.Parameters.Count() == 2
                    && cmd.CommandText.Trim().Equals("select u.userid from BPAUser u where u.authenticationServerClientId = @authServerClientId and u.authtype = @authType", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable.SingleCell("userid", TestUserId));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.Parameters.OfType<IDataParameter>().FirstOrDefault()?.Value is Guid id && id == TestUserId
                    && cmd.CommandText.Trim().EndsWith("where u.userid in (@id1)", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable
                    .WithColumns("userid", "username", "loginattempts", "isdeleted", "created", "expiry", "passwordexpiry", "passworddurationweeks", "alerteventtypes", "alertnotificationtypes", "lastsignedin", "authtype", "passwordexpirywarninginterval", "locked")
                    .AddRow(TestUserId, "!!!TEST!!!", 0, 0, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1), 4, 0, 0, DateTime.UtcNow.AddDays(-1), 1, 0, 0));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().StartsWith("select r.name from BPAUser u", StringComparison.OrdinalIgnoreCase)
                    && cmd.CommandText.Trim().EndsWith("where u.userid = @id", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable
                    .WithColumns("name")
                    .AddRow("TESTROLE"));

            var hash = new PBKDF2PasswordHasher().GenerateHash(TestPassword.AsSecureString(), out var salt);
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.Parameters.OfType<IDataParameter>().FirstOrDefault()?.Value is Guid id && id == TestUserId
                    && cmd.CommandText.Trim().Equals("select type, salt, hash from BPAPassword where active = 1 and userid = @userid", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable
                    .WithColumns("type", "salt", "hash")
                    .AddRow(1, salt, hash));

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("select ActiveDirectoryProvider from BPASysConfig", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar("");

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("SELECT EnforceEditSummaries FROM BPASysConfig", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar(false);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("SELECT CompressProcessXML FROM BPASysConfig", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar(false);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.StartsWith("select ActiveDirectoryProvider, PopulateUserNameUsing,", StringComparison.OrdinalIgnoreCase)
                    && cmd.CommandText.EndsWith("from BPASysConfig", StringComparison.OrdinalIgnoreCase))
                .ReturnsTable(MockTable.WithColumns("activedirectoryprovider", "populateusernameusing", "showusernamesonlogin", "authenticationgatewayurl", "enablemappedactivedirectoryauth", "enableexternalauth", "authenticationserverurl", "enableauthenticationserverauth", "authenticationserverapicredential")
                .AddRow("", 0, false, "", false, false, "https://localhost/", true, Guid.NewGuid()));
        }

        private void ConfigureGroupMocks()
        {
            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("select groupid from BPVGroupedResources where Id = @Id", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar(DBNull.Value);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().EndsWith("Select top 1 id from groups where isrestricted=1 and id <> @groupID", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar(DBNull.Value);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("select isrestricted from BPAGroup  where id=@group", StringComparison.OrdinalIgnoreCase))
                .ReturnsScalar(false);
        }

        private void ConfigurePermissionMocks()
        {
            var permissionsMockTable = MockTable.WithColumns("id", "name", "requiredFeature");
            var permissionGroupsMockTable = MockTable.WithColumns("id", "name", "permid", "requiredFeature");
            var userRoleMockTable = MockTable.WithColumns("rolename", "permname");
            var permissionId = 1;
            foreach(var permission in PermissionList.Permissions)
            {
                permissionsMockTable.AddRow(permissionId, permission, "");
                permissionGroupsMockTable.AddRow(1, "TESTGROUP", permissionId, "");
                userRoleMockTable.AddRow("TESTROLE", permission);
                ++permissionId;
            }

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().Equals("select p.id, p.name, p.requiredFeature from BPAPerm p; select pg.id, pg.name, pgm.permid, pg.requiredFeature from BPAPermGroup pg   join BPAPermGroupMember pgm on pgm.permgroupid = pg.id;", StringComparison.OrdinalIgnoreCase))
                .ReturnsDataset(permissionsMockTable, permissionGroupsMockTable);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Trim().StartsWith("select r.id, r.name, r.ssogroup, r.requiredFeature from BPAUserRole r", StringComparison.OrdinalIgnoreCase))
                .ReturnsDataset(
                    MockTable
                        .WithColumns("id", "name", "ssogroup", "requiredFeature")
                        .AddRow(1, "TESTROLE", DBNull.Value, ""),
                    userRoleMockTable
                );

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.Equals("delete from BPAUserExternalReloginToken where tokenexpiry < getutcdate()", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar(1);

            DatabaseConnection.Mocks
                .When(cmd =>
                    cmd.HasValidSqlServerCommandText()
                    && cmd.CommandText.StartsWith("delete from BPAUserExternalReloginToken", StringComparison.InvariantCultureIgnoreCase))
                .ReturnsScalar(1);
        }
    }
}
