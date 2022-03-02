using FluentAssertions;
using NUnit.Framework;

namespace AutomateUI.UnitTests.Classes
{
    public class NewActiveDirectoryUserViewModelTests
    {
        [Test]
        public void NewActiveDirectoryUserViewModel_CreatedWithNoParameters_PropertiesShouldBeNull()
        {
            var newActiveDirectoryUser = new NewActiveDirectoryUserViewModel();
            _ = newActiveDirectoryUser.Sid.Should().BeNull();
            _ = newActiveDirectoryUser.Upn.Should().BeNull();
        }

        [Test]
        public void NewActiveDirectoryUserViewModel_CreatedWithParameters_PropertiesShouldBeAssigned()
        {
            var sid = "S-1-5-32-1045337234-12924708993-5683276719-19000";
            var upn = "test@testdomain.internal";
            var newActiveDirectoryUser = new NewActiveDirectoryUserViewModel(sid, upn);

            _ = newActiveDirectoryUser.Sid.Should().Be(sid);
            _ = newActiveDirectoryUser.Upn.Should().Be(upn);
        }
    }
}
