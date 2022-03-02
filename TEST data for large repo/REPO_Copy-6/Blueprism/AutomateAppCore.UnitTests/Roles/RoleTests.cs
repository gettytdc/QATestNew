using BluePrism.AutomateAppCore.Auth;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Roles
{
    [TestFixture]
    public class RoleTests
    {
        [Test(Description = "Tests that a sysadmin role has the correct restrictions on it")]
        public void TestSysadminRole()
        {
            var r = new Role(Role.DefaultNames.SystemAdministrators);
            Assert.That(r.CanChangeActiveDirectoryGroup, Is.True);
            Assert.That(r.CanChangePermissions, Is.False);
            Assert.That(r.CanDelete, Is.False);
            Assert.That(r.CanRename, Is.False);
        }

        [Test(Description = "Tests that a runtime resource role has the correct restrictions on it")]
        public void TestRuntimeResourceRole()
        {
            var r = new Role(Role.DefaultNames.RuntimeResources);
            Assert.That(r.CanChangeActiveDirectoryGroup, Is.True);
            Assert.That(r.CanChangePermissions, Is.False);
            Assert.That(r.CanDelete, Is.False);
            Assert.That(r.CanRename, Is.False);
        }
    }
}
