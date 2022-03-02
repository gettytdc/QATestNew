#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Groups;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.UnitTesting;
using BluePrism.Data;
using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests
{
    [TestFixture]
    public class ClsServerGroupTreeTests
    {
        private clsServer _server;
        private Mock<IDatabaseConnection> _databaseConnectionMock;
        private IUser _loggedInUser;

        [SetUp]
        public void Setup()
        {

            LegacyUnitTestHelper.SetupDependencyResolver();

            _server = new clsServer();

            _databaseConnectionMock = new Mock<IDatabaseConnection>();
            _databaseConnectionMock.Setup(x => x.Execute(It.IsAny<SqlCommand>()));

            _loggedInUser = User.CreateEmptyMappedActiveDirectoryUser(Guid.NewGuid());
            _loggedInUser.Name = "Testy McTesterson";
            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", _server, (Func<IDatabaseConnection>)(() => _databaseConnectionMock.Object));
            ReflectionHelper.SetPrivateField("mLoggedInUser", _server, _loggedInUser);
        }

        [Test]
        public void ClsServer_GetGroupIdParameterTable_RowsShouldMatchIEnumerableCountGiven()
        {
            var listOfGuids = new List<Guid>
            {
                new Guid(),
                new Guid()
            };

            var dataTable = (DataTable)ReflectionHelper.InvokePrivateMethod("GetGroupIdParameterTable", _server, listOfGuids);

            Assert.That(dataTable.Rows.Count, Is.EqualTo(listOfGuids.Count));
        }

        [Test]
        public void ClsServer_SaveTreeNodeExpandedState_ShouldNotThrow()
        {
            Action test = () =>
            {
                _server.SaveTreeNodeExpandedState(new Guid(), true, It.IsAny<GroupTreeType>());
            };
            test.ShouldNotThrow();
        }
    }
}
#endif
