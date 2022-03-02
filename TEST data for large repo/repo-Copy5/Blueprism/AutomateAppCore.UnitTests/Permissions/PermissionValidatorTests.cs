using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Permissions
{

    /// <summary>
    /// Tests aspects of the permission helper interface class.
    /// </summary>
    [TestFixture]
    public class PermissionValidatorTests
    {
        private PermissionValidator _sut;
        private ServerPermissionsContext _context;

        [SetUp]
        public void SetUp()
        {
            _sut = new PermissionValidator();
            _context = new ServerPermissionsContext();
        }


        [Test]
        public void LocalRequest_AllowAnyLocalCalls_PassesPermissionCheck()
        {
            _context.AllowAnyLocalCalls = true;
            _context.IsLocal = true;

            Assert.DoesNotThrow(() => _sut.EnsurePermissions(_context));
        }

        [Test]
        public void RemoteRequest_AllowAnyLocalCalls_FailsPermissionCheck()
        {
            _context.AllowAnyLocalCalls = true;

            Assert.Throws<PermissionException>(() => _sut.EnsurePermissions(_context));
        }

        [Test]
        public void LocalRequest_ValidateLocalCalls_FailsPermissionCheck()
        {
            _context.IsLocal = true;

            Assert.Throws<PermissionException>(() => _sut.EnsurePermissions(_context));
        }

        [Test]
        public void UserNotLoggedIn_ThrowsPermissionException()
        {
            Assert.Throws<PermissionException>(() => _sut.EnsurePermissions(_context), "Unauthorized - user not logged in.");
        }

        [Test]
        public void NoPermissionRestrictions_EmptyCollection_PassesPermissionCheck()
        {
            var user = new Mock<IUser>();
            _context.User = user.Object;
            _context.Permissions = new string[] { };

            Assert.DoesNotThrow(() => _sut.EnsurePermissions(_context));
        }

        [Test]
        public void NoPermissionRestrictions_NullCollection_PassesPermissionCheck()
        {
            var user = new Mock<IUser>();
            _context.User = user.Object;

            Assert.DoesNotThrow(() => _sut.EnsurePermissions(_context));
        }

        [Test]
        public void UserDoesNotHavePermission_RequiresPermission_ThrowsPermissionException()
        {
            var permission = "requiredPermission";
            var user = new Mock<IUser>();
            user.Setup(u => u.HasPermission(permission)).Returns(false);

            _context.User = user.Object;
            _context.Permissions = new string[] { permission };

            Assert.Throws<PermissionException>(() => _sut.EnsurePermissions(_context), "Unauthorized - user does not have permission.");
        }

        [Test]
        public void UserHasRequiredPermission_PassesPermissionCheck()
        {
            var permission = "requiredPermission";
            var user = new Mock<IUser>();
            user.Setup(u => u.HasPermission(permission)).Returns(true);

            _context.User = user.Object;
            _context.Permissions = new string[] { permission };

            Assert.DoesNotThrow(() => _sut.EnsurePermissions(_context));
        }
    }

}
