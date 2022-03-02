#if UNITTESTS

using System.Security.Principal;
using BluePrism.Core.WindowsSecurity;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.WindowsSecurity
{
    public class UserAccountIdentifierTests
    {
        private static readonly string CurrentUserName;
        private static readonly string CurrentUserSid;

        static UserAccountIdentifierTests()
        {
            var windowsIdentity = WindowsIdentity.GetCurrent();
            CurrentUserName = windowsIdentity.Name;
            CurrentUserSid = windowsIdentity.User.Value;
            
        }

        [Test]
        public void ShouldCreateFromSid()
        {
            var account = UserAccountIdentifier.CreateFromSid(CurrentUserSid);

            Assert.That(account.Sid, Is.EqualTo(CurrentUserSid));
            Assert.That(account.Name, Is.EqualTo(CurrentUserName));
        }

        [Test]
        public void ShouldCreateFromUserName()
        {
            var account = UserAccountIdentifier.CreateFromAccountName(CurrentUserName);

            Assert.That(account.Sid, Is.EqualTo(CurrentUserSid));
            Assert.That(account.Name, Is.EqualTo(CurrentUserName));
        }

        [Test]
        public void AccountsWithSameSidsShouldBeEqual()
        {
            Assert.That(new UserAccountIdentifier("User1", "sid1"), Is.EqualTo(new UserAccountIdentifier("User1", "sid1")));
            Assert.That(new UserAccountIdentifier("User1", "sid1"), Is.EqualTo(new UserAccountIdentifier("User1 Different Name", "sid1")));
        }

        [Test]
        public void AccountsWithDifferentSidsShouldNotBeEqual()
        {
            Assert.That(new UserAccountIdentifier("User1", "sid1"), Is.Not.EqualTo(new UserAccountIdentifier("User2", "sid2")));
            Assert.That(new UserAccountIdentifier("User1", "sid1"), Is.Not.EqualTo(new UserAccountIdentifier("User1", "sid2")));
        }

        
    }
}

#endif