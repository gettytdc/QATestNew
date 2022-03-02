namespace AutomateAppCore.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using BluePrism.AutomateAppCore;
    using BluePrism.AutomateAppCore.Auth;
    using BluePrism.AutomateAppCore.clsServerPartialClasses.Caching;
    using BluePrism.Utilities.Functional;
    using BluePrism.Core.Utility;
    using BluePrism.Data;
    using BluePrism.Utilities.Testing;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CacheDataProviderTests : UnitTestBase<CacheDataProvider>
    {
        [Test]
        public void GetAllGroupPermissionsReurnsExpectedGroupPermissions()
        {
            var group1Id = Guid.NewGuid();
            var group2Id = Guid.NewGuid();
            var group3Id = Guid.NewGuid();

            (Guid Id, int? UserRoleId, string Name)[] permissions =
            {
                (group1Id, 1, "Test Role 1"),
                (group1Id, 2, "Test Role 2"),
                (group1Id, 3, "Test Role 3"),
                (group2Id, 2, "Test Role 2"),
                (group3Id, null, null)
            };

            var dataReaderMock = GetMock<IDataReader>();
            SetupPermissionsDataReader(dataReaderMock, permissions);

            var databaseCommandMock = GetMock<IDbCommand>();

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(databaseCommandMock.Object))
                .Returns(() => dataReaderMock.Object);

            var result = ClassUnderTest.GetAllGroupPermissions(databaseConnectionMock.Object);

            var roles = new[]
            {
                new GroupLevelPermissions(1, "Test Role 1"),
                new GroupLevelPermissions(2, "Test Role 2"),
                new GroupLevelPermissions(3, "Test Role 3")
            };
            Action<GroupPermissions> AddRoles(params int[] roleIds) => gp =>
                roleIds.Select(id => roles.Single(x => x.Id == id)).ForEach(gp.Add).Evaluate();

            var expectedResult = new Dictionary<string, IGroupPermissions>
            {
                { group1Id.ToString(), new GroupPermissions(group1Id, PermissionState.Restricted).Tee(AddRoles(1, 2, 3)) },
                { group2Id.ToString(), new GroupPermissions(group2Id, PermissionState.Restricted).Tee(AddRoles(2)) },
                { group3Id.ToString(), new GroupPermissions(group3Id, PermissionState.Restricted) }
            };

            result.Should().Equal(expectedResult);
        }

        [Test]
        public void GetProcessGroupsReturnsExpectedProcessGroups()
        {
            var process1Id = Guid.NewGuid();
            var process2Id = Guid.NewGuid();

            var group1Id = Guid.NewGuid();
            var group2Id = Guid.NewGuid();
            var group3Id = Guid.NewGuid();

            (Guid ProcessId, Guid GroupId)[] data =
            {
                (process1Id, group1Id),
                (process1Id, group2Id),
                (process1Id, group3Id),
                (process2Id, group2Id)
            };

            var dataReaderMock = GetMock<IDataReader>();
            SetupGroupedDataDataReader(dataReaderMock, data);

            var databaseCommandMock = GetMock<IDbCommand>();

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(databaseCommandMock.Object))
                .Returns(() => dataReaderMock.Object);

            var result = ClassUnderTest.GetProcessGroups(databaseConnectionMock.Object);

            var expectedResult = new Dictionary<Guid, List<Guid>>
            {
                {process1Id, new List<Guid> {group1Id, group2Id, group3Id}},
                {process2Id, new List<Guid> {group2Id}}
            };

            result.Should().Equal(expectedResult, (x, y) => x.Key.Equals(y.Key) && x.Value.ElementsEqual(y.Value));
        }

        [Test]
        public void GetResourceGroupsReturnsExpectedResourceGroups()
        {
            var process1Id = Guid.NewGuid();
            var process2Id = Guid.NewGuid();

            var group1Id = Guid.NewGuid();
            var group2Id = Guid.NewGuid();
            var group3Id = Guid.NewGuid();

            (Guid ProcessId, Guid GroupId)[] data =
            {
                (process1Id, group1Id),
                (process1Id, group2Id),
                (process1Id, group3Id),
                (process2Id, group2Id)
            };

            var dataReaderMock = GetMock<IDataReader>();
            SetupGroupedDataDataReader(dataReaderMock, data);

            var databaseCommandMock = GetMock<IDbCommand>();

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            databaseConnectionMock
                .Setup(m => m.ExecuteReturnDataReader(databaseCommandMock.Object))
                .Returns(() => dataReaderMock.Object);

            var result = ClassUnderTest.GetResourceGroups(databaseConnectionMock.Object);

            var expectedResult = new Dictionary<Guid, List<Guid>>
            {
                {process1Id, new List<Guid> {group1Id, group2Id, group3Id}},
                {process2Id, new List<Guid> {group2Id}}
            };

            result.Should().Equal(expectedResult, (x, y) => x.Key.Equals(y.Key) && x.Value.ElementsEqual(y.Value));
        }

        private static void SetupPermissionsDataReader(
            Mock<IDataReader> dataReaderMock,
            IReadOnlyCollection<(Guid Id, int? UserRoleId, string Name)> permissions)
        {
            permissions
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m.Read()),
                    (m, _) => m.Returns(true))
                .Returns(false);

            permissions
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m["id"]),
                    (m, i) => m.Returns(i.Id));

            permissions
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m["userroleid"]),
                    (m, i) => m.Returns((object)i.UserRoleId ?? DBNull.Value));

            permissions
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m.IsDBNull(2)),
                    (m, i) => m.Returns(i.UserRoleId == null));
            
            dataReaderMock
                .Setup(m => m.IsDBNull(3))
                .Returns(true);

            permissions
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m["name"]),
                    (m, i) => m.Returns(i.Name));
        }

        private static void SetupGroupedDataDataReader(
            Mock<IDataReader> dataReaderMock,
            IReadOnlyCollection<(Guid ProcessId, Guid GroupId)> groupedData)
        {
            groupedData
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m.Read()),
                    (m, _) => m.Returns(true))
                .Returns(false);

            groupedData
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m["groupId"]),
                    (m, i) => m.Returns(i.GroupId));

            groupedData
                .Aggregate(
                    dataReaderMock.SetupSequence(m => m["id"]),
                    (m, i) => m.Returns(i.ProcessId));
        }
    }
}
