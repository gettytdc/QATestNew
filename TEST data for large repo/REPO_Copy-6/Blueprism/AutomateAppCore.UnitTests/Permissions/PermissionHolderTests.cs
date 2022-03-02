using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.Permissions
{
    /// <summary>
    /// Tests aspects of the permission holder.
    /// </summary>
    [TestFixture]
    public class PermissionHolderTests
    {

        [Test]
        public void TestEqualsForPermHolder_Nothing()
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var role1 = new Role("Test1")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };
            Role role2 = null;

            Assert.IsFalse(role1.Equals(role2));
        }

        [Test]
        public void TestEqualsForPermHolder_Empty()
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var role1 = new Role("Test1")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };
            var role2 = new Role("Test2");

            Assert.IsFalse(role1.Equals(role2));
        }

        [Test]
        public void TestEqualsForPermHolder_DiffRole()
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var role1 = new Role("Test1")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };
            var role2 = new Role("Test2")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };

            Assert.IsFalse(role1.Equals(role2));
        }

        [Test]
        public void TestEqualsForPermHolder_SameRole()
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var role1 = new Role("Test1")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };
            var role2 = new Role("Test2")
            {
                Permission.ControlRoom.ManageQueuesReadOnly
            };

            Assert.IsFalse(role1.Equals(role2));
        }

        [Test]
        public void TestEqualsForPermHolder_DiffRoleDifferms()
        {
            var mockServer = new Mock<IServer>();
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var role1 = new Role("Test1")
            {
                Permission.ControlRoom.ManageQueuesFullAccess
            };
            var role2 = new Role("Test2")
            {
            Permission.ControlRoom.ManageQueuesReadOnly
            };

            Assert.IsFalse(role1.Equals(role2));
        }

        private PermissionData CreatePermissionDataObject()
        {
            var perms = new Dictionary<int, Permission>
            {
                { 83, Permission.CreatePermission(83, Permission.ProcessStudio.ManageProcessAccessRights) },
                { 73, Permission.CreatePermission(73, Permission.ProcessStudio.EditProcessGroups) },
                { 56, Permission.CreatePermission(56, Permission.ObjectStudio.ExecuteBusinessObject) },
                { 26, Permission.CreatePermission(26, Permission.ObjectStudio.EditBusinessObject) },
                { 27, Permission.CreatePermission(27, Permission.ProcessStudio.EditProcess) },
                { 57, Permission.CreatePermission(57, Permission.ProcessStudio.ExecuteProcess) },

                { 19, Permission.CreatePermission(19, Permission.ObjectStudio.CreateBusinessObject) },
                { 20, Permission.CreatePermission(20, Permission.ProcessStudio.CreateProcess) }
            };
            var groups = new Dictionary<int, PermissionGroup>
            {
                { 4, new PermissionGroup(4, "Process Studio") },
                { 2, new PermissionGroup(4, "Object Studio") }
            };
            var retval = new PermissionData(perms, groups);
            return retval;
        }
    }

}
