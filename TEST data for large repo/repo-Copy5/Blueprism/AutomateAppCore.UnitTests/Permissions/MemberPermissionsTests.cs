#if UNITTESTS
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Server.Domain.Models;
using BluePrism.Utilities.Testing;
using BluePrism.Utilities.Functional;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace AutomateAppCore.UnitTests.Permissions
{
    public class MemberPermissionsTests : UnitTestBase<MemberPermissions>
    {
        private IGroupPermissions _groupPermissions;

        protected override MemberPermissions TestClassConstructor()
        {
            return new MemberPermissions(_groupPermissions);
        }

        [Test]
        [TestCaseSource(nameof(HasPermissionTestCaseGenerator))]
        public void HasPermissionTest((bool userHasPermission, bool isSystemAdmin, bool isSystemUser, PermissionState permissionState, Permission requestPermission, bool expectedResult) parameters)
        {
            var mockPermissionData = new PermissionData(Permissions, Groups);
            var mockServer = new Mock<IServer>();
            mockServer.Setup(m => m.GetPermissionData()).Returns(mockPermissionData);
            var roles = new RoleSet() { new Role("Test") { Id = 0 } };
            Permission.Init(mockServer.Object);
            var userMock = new Mock<IUser>();
            userMock.Setup(m => m.HasPermission(It.IsAny<ICollection<Permission>>())).Returns(parameters.userHasPermission);
            userMock.SetupGet(m => m.IsSystemAdmin).Returns(parameters.isSystemAdmin);
            if (parameters.isSystemUser)
            {
                userMock.SetupGet(m => m.AuthType).Returns(AuthMode.System);
            }
            else
            {
                userMock.SetupGet(m => m.AuthType).Returns(AuthMode.Native);
            }

            userMock.SetupGet(m => m.Roles).Returns(roles);
            var groupPermissionsMock = new Mock<IGroupPermissions>();
            groupPermissionsMock.SetupGet(m => m.State).Returns(parameters.permissionState);
            groupPermissionsMock.Setup(m => m.GetEnumerator()).Returns(() => new[] { GroupLevelPermissions }.ToList().GetEnumerator());
            _groupPermissions = groupPermissionsMock.Object;
            bool result = ClassUnderTest.HasPermission(userMock.Object, parameters.requestPermission.Name);
            result.Should().Be(parameters.expectedResult);
        }

        [Test]
        public void HasPermissionWithMultiplePermissions()
        {
            var mockPermissionData = new PermissionData(Permissions, Groups);
            var serverMock = new Mock<IServer>();
            serverMock.Setup(m => m.GetPermissionData()).Returns(mockPermissionData);
            var roles = new RoleSet() { new Role("Test") { Id = 0 } };
            Permission.Init(serverMock.Object);
            var userMock = new Mock<IUser>();
            userMock.Setup(m => m.HasPermission(It.IsAny<ICollection<Permission>>())).Returns(true);
            userMock.Setup(m => m.Roles).Returns(roles);
            var groupPermissionsMock = new Mock<IGroupPermissions>();
            groupPermissionsMock.SetupGet(m => m.State).Returns(PermissionState.Restricted);
            groupPermissionsMock.Setup(m => m.GetEnumerator()).Returns(() => new[] { GroupLevelPermissions }.ToList().GetEnumerator());
            _groupPermissions = groupPermissionsMock.Object;
            bool result = ClassUnderTest.HasPermission(userMock.Object, InvalidPermission, Permissions[1]);
            result.Should().BeTrue();
        }

        protected static IEnumerable<(bool userHasPermission, bool isSystemAdmin, bool isSystemUser,
            PermissionState permissionState, Permission requestPermission,
            bool expectedResult)> HasPermissionTestCaseGenerator()
        {
            return new[] { (false, false, false, PermissionState.UnRestricted, InvalidPermission, false),
                (true, false, false, PermissionState.UnRestricted, InvalidPermission, true),
                (true, false, false, PermissionState.Restricted, InvalidPermission, false),
                (true, false, false, PermissionState.Restricted, Permissions[0], true),
                (true, false, false, PermissionState.Restricted, Permissions[1], true),
                (true, false, false, PermissionState.Restricted, Permissions[2], true),
                (true, false, false, PermissionState.RestrictedByInheritance, InvalidPermission, false),
                (true, false, false, PermissionState.RestrictedByInheritance, Permissions[0], true),
                (true, true, false, PermissionState.Restricted, InvalidPermission, true),
                (true, false, true, PermissionState.Restricted, InvalidPermission, true) };
        }

        private static readonly Permission InvalidPermission = Permission.CreatePermission(123, "Invalid");
        private static readonly Dictionary<int, Permission> Permissions = (new string[] { "Test", "Test2", "Test3" })
            .Map(x => AssignIndexes(x))
            .Select(x => Permission.CreatePermission(x.index, x.value))
            .Concat(new[] { InvalidPermission }).ToDictionary(k => k.Id, v => v);
        private static readonly Dictionary<int, PermissionGroup> Groups = (new string[] { "Test", "Test2", "Test3" })
            .Map(x => AssignIndexes(x))
            .Select(x => new PermissionGroup(x.index, x.value)).ToDictionary(k => k.Id, v => v);
        private static readonly GroupLevelPermissions GroupLevelPermissions = Permissions.Values.Where(x => !x.Equals(InvalidPermission))
            .Map(x => new GroupLevelPermissions(0).Tee(glp => glp.AddAll(x)));

        private static IEnumerable<(int index, T value)> AssignIndexes<T>(IEnumerable<T> items)
        {
            return items.Map(x => (x, Enumerable.Range(0, x.Count()))).Map(x => x.Item2.Zip(x.Item1, (i1, i2) => (i1, i2)));
        }

    }
}
#endif
