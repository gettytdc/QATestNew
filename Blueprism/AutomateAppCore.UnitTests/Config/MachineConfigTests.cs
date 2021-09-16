using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.Common.Security;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using BluePrism.ClientServerResources.Core.Config;
using static BluePrism.AutomateAppCore.MachineConfig;
using BluePrism.ClientServerResources.Core.Enums;
using System.Security.Cryptography.X509Certificates;

namespace AutomateAppCore.UnitTests.Config
{
    [TestFixture]
    public class MachineConfigTests : UnitTestBase<MachineConfig>
    {
        [Test]
        public void EmptyMachineConfigTest()
        {
            var configLocatorMock = GetMock<IConfigLocator>();
            var machineConfig = new MachineConfig(configLocatorMock.Object);
            machineConfig.Connections.Should().BeEmpty();
            machineConfig.Servers.Should().BeEmpty();
        }

        [Test]
        public void AddConnectionTest()
        {
            var configLocatorMock = GetMock<IConfigLocator>();
            var machineConfig = new MachineConfig(configLocatorMock.Object);
            machineConfig.AddConnection(new clsDBConnectionSetting("Test") { ConnectionType = ConnectionType.Availability });
            machineConfig.Connections.Count.Should().Be(1);
            machineConfig.GetConnection("Test").Should().NotBeNull();
        }

        /// <summary>
        /// The config objects returned out of <see cref="LoadMachineConfigTestCases"/> should match the LoadTestX.config files in ".\AutomateAppCore.UnitTests\Config\LoadMachineConfigData"
        /// </summary>
        [TestCaseSource(nameof(LoadMachineConfigTestCases))]
        public void LoadMachineConfigTest(MachineConfigTestData testData)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\LoadMachineConfigData", testData.Filename);
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Setup
                var configLocatorMock = GetMock<IConfigLocator>();
                var machineConfigMock = new Mock<MachineConfig>(configLocatorMock.Object)
                {
                    CallBase = true
                };


                machineConfigMock.Setup(a => a.OpenReadFile()).Returns(fileStream);
                var machineConfig = machineConfigMock.Object;

                // Execute
                machineConfig.Load();

                // Verify
                VerifyConfigs(machineConfig, testData);
            }
        }

        [TestCaseSource(nameof(LoadMachineConfigTestCases))]
        public void SaveMachineConfigTest(MachineConfigTestData testData)
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\SaveMachineConfigData");
            var configLocatorMock = GetMock<IConfigLocator>();
            configLocatorMock.Setup(a => a.MachineConfigDirectory).Returns(new DirectoryInfo(filePath));
            var machineConfigMock = new Mock<MachineConfig>(configLocatorMock.Object)
            {
                CallBase = true
            };

            var machineConfig = machineConfigMock.Object;

            testData.ExpectedConnections.ForEach(c => machineConfig.AddConnection(c));
            testData.ExpectedServers.ForEach(s => machineConfig.AddServer(s));

            // Execute
            machineConfig.Save();
            machineConfig.Load();

            // Verify
            VerifyConfigs(machineConfig, testData);
        }

        [Test]
        public void Save_WillSetXmlPropertiesCorrectly()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\SaveMachineConfigData");
            var configLocatorMock = GetMock<IConfigLocator>();
            configLocatorMock.Setup(a => a.MachineConfigDirectory).Returns(new DirectoryInfo(filePath));
            var machineConfigMock = new Mock<MachineConfig>(configLocatorMock.Object)
            {
                CallBase = true
            };

            var machineConfig = machineConfigMock.Object;

            var editSummariesAreCompulsory = false;
            var startProcessEngine = true;
            var sqlCommandTimeout = 999;
            var sqlCommandTimeoutLong = 999999;
            var sqlCommandTimeoutLog = 43;
            var databaseInstallCommandTimeout = 761;
            var dataPipelineProcessCommandSendTimeout = 321;
            var archivePath = "archivePath";

            machineConfig.EditSummariesAreCompulsory = editSummariesAreCompulsory;
            machineConfig.StartProcessEngine = startProcessEngine;
            machineConfig.SqlCommandTimeout = sqlCommandTimeout;
            machineConfig.SqlCommandTimeoutLong = sqlCommandTimeoutLong;
            machineConfig.SqlCommandTimeoutLog = sqlCommandTimeoutLog;
            machineConfig.DatabaseInstallCommandTimeout = databaseInstallCommandTimeout;
            machineConfig.DataPipelineProcessCommandSendTimeout = dataPipelineProcessCommandSendTimeout;
            machineConfig.ArchivePath = archivePath;

            machineConfig.Save();
            machineConfig.Load();

            machineConfig.EditSummariesAreCompulsory.Should().Be(editSummariesAreCompulsory);
            machineConfig.StartProcessEngine.Should().Be(startProcessEngine);
            machineConfig.SqlCommandTimeout.Should().Be(sqlCommandTimeout);
            machineConfig.SqlCommandTimeoutLong.Should().Be(sqlCommandTimeoutLong);
            machineConfig.SqlCommandTimeoutLog.Should().Be(sqlCommandTimeoutLog);
            machineConfig.DatabaseInstallCommandTimeout.Should().Be(databaseInstallCommandTimeout);
            machineConfig.DataPipelineProcessCommandSendTimeout.Should().Be(dataPipelineProcessCommandSendTimeout);
            machineConfig.ArchivePath.Should().Be(archivePath);
        }

        public class UpdateDatabaseSettingsMethods : UnitTestBase<MachineConfig>
        {
            private MachineConfig _machineConfig;

            [SetUp]
            public void SetUp()
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Config\SaveMachineConfigData");
                var configLocatorMock = GetMock<IConfigLocator>();
                configLocatorMock.Setup(a => a.MachineConfigDirectory).Returns(new DirectoryInfo(filePath));
                var machineConfigMock = new Mock<MachineConfig>(configLocatorMock.Object)
                {
                    CallBase = true
                };

                _machineConfig = machineConfigMock.Object;
            }

            [Test]
            public void UpdateDatabaseSettings_IfConnectionAlreadyExists_WillUpdateSettingsCorrectly()
            {
                var connection = CreateConnectionSetting("TestConnection");
                _machineConfig.AddConnection(connection);

                var callbackPort = 1235;
                var address = "127.0.0.1";
                var port = 1234;
                var connectionMode = ServerConnection.Mode.WCFInsecure;
                // Execute
                _machineConfig.UpdateDatabaseSettings(connection.ConnectionName, address, port,
                   connectionMode, callbackPort);
                _machineConfig.Load();

                // Verify
                var updatedConnection = _machineConfig.Connections.SingleOrDefault(c => c.ConnectionName == connection.ConnectionName);
                updatedConnection.CallbackPort.Should().Be(callbackPort);
                updatedConnection.ConnectionType.Should().Be(ConnectionType.BPServer);
                updatedConnection.DBServer.Should().Be(address);
                updatedConnection.Port.Should().Be(port);
                updatedConnection.ConnectionMode.Should().Be(connectionMode);
            }
          
            [Test]
            public void UpdateDatabaseSettings_IfConnectionDoesntExist_WillCreateOneWithCorrectSettings()
            {
                var newConnectionName = "New Connection";
                var callbackPort = 1235;
                var address = "127.0.0.1";
                var port = 1234;
                var connectionMode = ServerConnection.Mode.WCFInsecure;
                // Execute
                _machineConfig.UpdateDatabaseSettings(newConnectionName, address, port,
                   connectionMode, callbackPort);
                _machineConfig.Load();

                var updatedConnection = _machineConfig.Connections.SingleOrDefault(c => c.ConnectionName == newConnectionName);
                updatedConnection.CallbackPort.Should().Be(callbackPort);
                updatedConnection.ConnectionType.Should().Be(ConnectionType.BPServer);
                updatedConnection.DBServer.Should().Be(address);
                updatedConnection.Port.Should().Be(port);
                updatedConnection.ConnectionMode.Should().Be(connectionMode);
            }

            [Test]
            public void UpdateDatabaseSettings_IfConnectionAlreadyExistsAndConfigHasDatabaseProperties_WillUpdateSettingsCorrectly()
            {
                var connectionName = "TestConnection";
                var connection = CreateConnectionSetting(connectionName);

                _machineConfig.AddConnection(connection);

                var safeString = new SafeString();
                var address = "127.0.0.1";
                var dbName = "TestUpdate";
                var username = "HelloWorld";
                var winAuth = false;
                // Execute
                _machineConfig.UpdateDatabaseSettings(connectionName, address, dbName, username, safeString, winAuth);
                _machineConfig.Load();

                // Verify
                var updatedConnection = _machineConfig.Connections.SingleOrDefault(c => c.ConnectionName == connectionName);
                updatedConnection.DBServer.Should().Be(address);
                updatedConnection.DatabaseName.Should().Be(dbName);
                updatedConnection.DBUserName.Should().Be(username);
                updatedConnection.DBUserPassword.Should().Be(safeString);
                updatedConnection.WindowsAuth.Should().Be(winAuth);
            }

            [Test]
            public void UpdateDatabaseSettings_IfConnectionDoesntExistAndConfigHasDatabaseProperties_WillUpdateSettingsCorrectly()
            {
                var connectionName = "Test Connection";
                var safeString = new SafeString();
                var address = "127.0.0.1";
                var dbName = "TestUpdate";
                var username = "HelloWorld";
                var winAuth = false;
                // Execute
                _machineConfig.UpdateDatabaseSettings(connectionName, address, dbName, username, safeString, winAuth);
                // Execute
                _machineConfig.UpdateDatabaseSettings("TestConnection", "127.0.0.1", "TestUpdate", "HelloWorld", safeString, false, 1234, false);
                _machineConfig.Load();

                // Verify
                var updatedConnection = _machineConfig.Connections.SingleOrDefault(c => c.ConnectionName == connectionName);
                updatedConnection.DBServer.Should().Be(address);
                updatedConnection.DatabaseName.Should().Be(dbName);
                updatedConnection.DBUserName.Should().Be(username);
                updatedConnection.DBUserPassword.Should().Be(safeString);
                updatedConnection.WindowsAuth.Should().Be(winAuth);
            }
        }

        public class MachineConfigTestData
        {
            public string Filename { get; set; }
            public List<clsDBConnectionSetting> ExpectedConnections { get; set; }
            public List<ServerConfig> ExpectedServers { get; set; }
        }

        private void VerifyConfigs(MachineConfig machineConfig, MachineConfigTestData testData)
        {
            machineConfig.Connections.Count.Should().Be(testData.ExpectedConnections.Count);
            foreach (var testConnection in testData.ExpectedConnections)
            {
                var connection = machineConfig.GetConnection(testConnection.ConnectionName);
                connection.Should().NotBeNull();
                connection.Should().Be(testConnection);
            }

            machineConfig.Servers.Count.Should().Be(testData.ExpectedServers.Count);
            foreach (var testServer in testData.ExpectedServers)
            {
                var server = machineConfig.GetServerConfig(testServer.Name);
                server.Should().NotBeNull();
                
                server.ShouldBeEquivalentTo(testServer, options => options
                                    .Excluding(x => x.SelectedMemberPath.EndsWith("BrokerPassword")));

                server.AuthenticationServerBrokerConfig?.BrokerPassword.Should().Be(testServer.AuthenticationServerBrokerConfig?.BrokerPassword);

            }
        }

        private static IEnumerable<MachineConfigTestData> LoadMachineConfigTestCases()
        {
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest1.config",
                ExpectedConnections = new List<clsDBConnectionSetting>(),
                ExpectedServers = new List<ServerConfig>()
            };
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest2.config",
                ExpectedConnections = new List<clsDBConnectionSetting>()
                {
                    new clsDBConnectionSetting("TestConnection")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        Ordered = false
                    }
                },
                ExpectedServers = new List<ServerConfig>()
            };
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest3.config",
                ExpectedConnections = new List<clsDBConnectionSetting>()
                {
                    new clsDBConnectionSetting("TestConnection")
                    { 
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        Ordered = false
                    }
                },
                ExpectedServers = new List<ServerConfig>()
                {
                    new ServerConfig(null)
                    {
                        Name = "TestServer",
                        Connection ="TestConnection",
                        Port = 8199,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        Verbose = true,
                        StatusEventLogging = true,
                        Ordered = false,
                        CallbackConnectionConfig = new ConnectionConfig()
                        {
                            CallbackProtocol = CallbackConnectionProtocol.Grpc,
                            HostName = "TestLocalhost",
                            Port = 8182,
                            CertificateName = "TestCertName",
                            ClientCertificateName = "TestClientCertName",
                            ServerStore = StoreName.My,
                            ClientStore = StoreName.My
                        }
                    }
                }
            };
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest4.config",
                ExpectedConnections = new List<clsDBConnectionSetting>()
                {
                    new clsDBConnectionSetting("ValidRabbitMqConfig")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        RabbitMqConfiguration = new RabbitMqConfiguration("rabbitmq://somebrokeraddress/","user1", "secretpassword".AsSecureString()),
                        Ordered = false
                    },

                    new clsDBConnectionSetting("RabbitMqConfigWithMissingUsername")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        RabbitMqConfiguration = null,
                        Ordered = false
                    },

                    new clsDBConnectionSetting("RabbitMqConfigWithMissingPassword")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        RabbitMqConfiguration = null,
                        Ordered = false
                    },

                    new clsDBConnectionSetting("RabbitMqConfigWithMissingHostUrl")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        RabbitMqConfiguration = null,
                        Ordered = false
                    },


                    new clsDBConnectionSetting("NoRabbitMqConfig")
                    {
                        ConnectionType = ConnectionType.BPServer,
                        Port = 8199,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        DBServer = "127.0.0.1",
                        RabbitMqConfiguration = null,
                        Ordered = false
                    }

                },
                ExpectedServers = new List<ServerConfig>()
                {
                    new ServerConfig(null)
                    {
                        Name = "TestServer",
                        Connection ="TestConnection",
                        Port = 8199,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        Verbose = true,
                        StatusEventLogging = true,
                        Ordered = false,
                        CallbackConnectionConfig = new ConnectionConfig()
                        {
                            CallbackProtocol = CallbackConnectionProtocol.Grpc,
                            HostName = "TestLocalhost",
                            Port = 8182,
                            CertificateName = "TestCertName",
                            ClientCertificateName = "TestClientCertName",
                            ServerStore = StoreName.CertificateAuthority,
                            ClientStore = StoreName.AuthRoot,
                        }
                    }
                }
            };
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest5.config",
                ExpectedConnections = new List<clsDBConnectionSetting>(),
                
                ExpectedServers = new List<ServerConfig>()
                {
                    new ServerConfig(null)
                    {
                        Name = "TestServer",
                        Connection ="TestConnection",
                        Port = 8199,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        Verbose = true,
                        StatusEventLogging = true,
                        Ordered = false,
                        AuthenticationServerBrokerConfig = new AuthenticationServerBrokerConfig("1", "2", "secretpassword".AsSecureString(), "4"),
                        CallbackConnectionConfig = new ConnectionConfig()
                        {
                            CallbackProtocol = CallbackConnectionProtocol.Grpc,
                            HostName = "TestLocalhost",
                            Port = 8182,
                            CertificateName = "TestCertName",
                            ClientCertificateName = "TestClientCertName",
                            ServerStore = StoreName.TrustedPublisher,
                            ClientStore = StoreName.TrustedPeople
                        }
                    }
                }
            };
            yield return new MachineConfigTestData()
            {
                Filename = "LoadTest5.config",
                ExpectedConnections = new List<clsDBConnectionSetting>(),
                ExpectedServers = new List<ServerConfig>()
                {
                    new ServerConfig(null)
                    {
                        Name = "TestServer",
                        Connection ="TestConnection",
                        Port = 8199,
                        MaxPendingChannels = 16384,
                        MaxTransferWindowSize = 4096,
                        ConnectionMode = ServerConnection.Mode.WCFInsecure,
                        Verbose = true,
                        StatusEventLogging = true,
                        Ordered = false,
                        AuthenticationServerBrokerConfig = new AuthenticationServerBrokerConfig("1", "2", "secretpassword".AsSecureString(), "4"),
                        CallbackConnectionConfig = new ConnectionConfig()
                        {
                            CallbackProtocol = CallbackConnectionProtocol.Grpc,
                            HostName = "TestLocalhost",
                            Port = 8182,
                            CertificateName = "TestCertName",
                            ClientCertificateName = "TestClientCertName",
                            ServerStore = StoreName.TrustedPublisher,
                            ClientStore = StoreName.TrustedPeople
                        }
                    }
                }
            };
        }

        private static clsDBConnectionSetting CreateConnectionSetting(string connectionName) => new clsDBConnectionSetting(connectionName)
        {
            ConnectionType = ConnectionType.BPServer,
            Port = 8199,
            ConnectionMode = ServerConnection.Mode.WCFInsecure,
            MaxPendingChannels = 16384,
            MaxTransferWindowSize = 4096,
            DBServer = "127.0.0.1",
            Ordered = false
        };
    }
}
