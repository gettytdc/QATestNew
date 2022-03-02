using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.Common.Security;
using BluePrism.Server.Domain.Models;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Models
{
    public class NativeAdminUserModelTests
    {
        [Test]
        public void Constructor_SetsUserAndPassword()
        {
            var user = new User(AuthMode.Native, Guid.NewGuid(), "User");
            var password = new SafeString("password");

            var nativeAdminUser = new NativeAdminUserModel(user, password);

            nativeAdminUser.User.Should().Be(user);
            nativeAdminUser.Password.Should().Be(password);
        }
    }
}
