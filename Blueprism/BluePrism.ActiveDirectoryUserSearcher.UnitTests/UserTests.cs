using BluePrism.ActiveDirectoryUserSearcher.Models;
using BluePrism.Core.ActiveDirectory.UserQuery;
using NUnit.Framework;

namespace BluePrism.ActiveDirectoryUserSearcher.UnitTests
{
    [TestFixture]
    public class UserTests
    {
        private ActiveDirectoryUser _activeDirectoryUser;
        private const string UPNName = "My UPN Name";
        private const string DistinguishedName = "My Distinguished Name";
        private const string SID = "1111-11-111";
        private const bool AlreadyMapped = false;
        private const bool IsChecked = false;

        [SetUp]
        public void SetUp()
        {
            _activeDirectoryUser = new ActiveDirectoryUser(UPNName, SID, DistinguishedName, AlreadyMapped);
        }

        [Test]
        public void Create_User_DetailsShouldBePopulated()
        {
            User user = new User(_activeDirectoryUser, IsChecked);
            Assert.AreEqual(UPNName, user.UserPrincipalName);
            Assert.AreEqual(DistinguishedName, user.DistinguishedName);
            Assert.AreEqual(SID, user.Sid);
            Assert.AreEqual(AlreadyMapped, user.AlreadyMapped);
            Assert.AreEqual(IsChecked, user.IsChecked);
        }
    }
}
