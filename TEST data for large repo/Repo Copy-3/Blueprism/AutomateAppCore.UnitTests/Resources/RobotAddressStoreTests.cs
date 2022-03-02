#if UNITTESTS
using System;
using System.Data;
using System.Threading;
using BluePrism.AutomateAppCore;
using BluePrism.Core.Resources;
using BluePrism.AutomateAppCore.Resources;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Resources
{
    [TestFixture]
    public class RobotAddressStoreTests
    {

        private DataTable _d1;
        private DataTable _d2;

        [SetUp]
        public void Setup()
        {
            _d1 = new DataTable();

            _d1.Columns.Add("resourceid", typeof(string));
            _d1.Columns.Add("name", typeof(string));
            _d1.Columns.Add("FQDN", typeof(string));
            _d1.Columns.Add("ssl", typeof(byte));

            _d2 = _d1.Clone();

            _d1.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8200", "BPEU611.", 0);
            _d1.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8201", "BPEU611.", 1);
            _d1.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8202", "", 0);

            _d2.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8200", "BPEU611.", 0);
            _d2.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8201", "BPEU611.", 1);
            _d2.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8202", "", 0);
            _d2.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8203", "BPEU611.", 1);
            _d2.Rows.Add(Guid.NewGuid().ToString(), "BPEU611:8204", "BPEU611.", 1);
        }

        [Test]
        [Description("Test that the data retrieved from the moq is serverd through the interface")]
        public void TestBasicFunction()
        {
            var server = new Mock<IServer>();

            server
                .Setup(x => x.GetResources(It.IsAny<ResourceAttribute>(), It.IsAny<ResourceAttribute>(), It.IsAny<string>()))
                .Returns(_d1);
            server
                .Setup(x => x.GetRequireSecuredResourceConnections())
                .Returns(true);
            server
                .Setup(x => x.GetResourceRegistrationMode())
                .Returns(ResourceRegistrationMode.FQDNFQDN);

            var sut = new RobotAddressStore(server.Object, 1);
            var result = sut.GetRobotAddress("BPEU611:8201");

            Assert.AreEqual(true, sut.RequireSecureResourceConnection, "Check requires secure is set");
            Assert.AreEqual(ResourceRegistrationMode.FQDNFQDN, sut.ResourceRegistrationMode, "Check registration mode is set");
            Assert.IsNotNull(result, "returned address was null");
            Assert.AreEqual("BPEU611.", result.FQDN);
            Assert.AreEqual("BPEU611.", result.HostName);
            Assert.AreEqual(8201, result.Port);
            Assert.AreEqual("BPEU611:8201", result.ResourceName);
            Assert.AreEqual(true, result.UseSsl);
        }

        [Test]
        [Description("Test that the data retrieved from the moq is served through the interface")]
        public void TestBasicFunctionOpposites()
        {
            var server = new Mock<IServer>();
            server
                .Setup(x => x.GetResources(It.IsAny<ResourceAttribute>(), It.IsAny<ResourceAttribute>(), It.IsAny<string>()))
                .Returns(_d1);
            server
                .Setup(x => x.GetRequireSecuredResourceConnections())
                .Returns(false);

            server
                .Setup(x => x.GetResourceRegistrationMode())
                .Returns(ResourceRegistrationMode.MachineMachine);

            var sut = new RobotAddressStore(server.Object, 1);

            Assert.AreEqual(false, sut.RequireSecureResourceConnection, "Check requires secure is set");
            Assert.AreEqual(ResourceRegistrationMode.MachineMachine, sut.ResourceRegistrationMode, "Check registration mode is set");
        }

        [Test]
        [Description("Test missing resource from server data returns nothing")]
        public void TestNotFoundRobotAddress()
        {
            var server = new Mock<IServer>();

            server
                .Setup(x => x.GetResources(It.IsAny<ResourceAttribute>(), It.IsAny<ResourceAttribute>(), It.IsAny<string>()))
                .Returns(_d1);
            server
                .Setup(x => x.GetRequireSecuredResourceConnections())
                .Returns(true);
            server
                .Setup(x => x.GetResourceRegistrationMode())
                .Returns(ResourceRegistrationMode.FQDNFQDN);

            var sut = new RobotAddressStore(server.Object, 1);
            var result = sut.GetRobotAddress("BPEU611:8299");

            Assert.AreEqual(true, sut.RequireSecureResourceConnection, "Check requires secure is set");
            Assert.AreEqual(ResourceRegistrationMode.FQDNFQDN, sut.ResourceRegistrationMode, "Check registration mode is set");
            Assert.IsNull(result, "returned address was not null");
        }

        [Test]
        [Description("Test that invalid data throws an exception")]
        public void TestNoFQDNThrowsErrorIfRegModeNotMAchineMachine()
        {
            var server = new Mock<IServer>();
            server
                .Setup(x => x.GetResources(It.IsAny<ResourceAttribute>(), It.IsAny<ResourceAttribute>(), It.IsAny<string>()))
                .Returns(_d1);
            server
                .Setup(x => x.GetRequireSecuredResourceConnections())
                .Returns(true);
            server
                .Setup(x => x.GetResourceRegistrationMode())
                .Returns(ResourceRegistrationMode.FQDNFQDN);

            var sut = new RobotAddressStore(server.Object, 120);         

            Assert.AreEqual(true, sut.RequireSecureResourceConnection, "Check requires secure is set");
            Assert.AreEqual(ResourceRegistrationMode.FQDNFQDN, sut.ResourceRegistrationMode, "Check registration mode is set");
            Assert.Throws(typeof(InvalidOperationException),  new TestDelegate(() => sut.GetRobotAddress("BPEU611:8202")));
        }

        [Test]
        [Description("Test that the info served is updated by the refresh")]
        public void TestRefresh()
        {
            var server = new Mock<IServer>();
            server
                .SetupSequence(x => x.GetResources(It.IsAny<ResourceAttribute>(), It.IsAny<ResourceAttribute>(), It.IsAny<string>()))
                .Returns(_d1)
                .Returns(_d1)
                .Returns(_d2);
            server
                .Setup(x => x.GetRequireSecuredResourceConnections())
                .Returns(true);
            server
                .Setup(x => x.GetResourceRegistrationMode())
                .Returns(ResourceRegistrationMode.FQDNFQDN);

            var sut = new RobotAddressStore(server.Object, 1);
            var result = sut.GetRobotAddress("BPEU611:8204");

            Assert.IsNull(result);

            Thread.Sleep(1005);

            result = sut.GetRobotAddress("BPEU611:8204");
            Assert.IsNotNull(result);
        }
    }
}
#endif
