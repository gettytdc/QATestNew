using System;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Auth
{
    public class AuthenticationServerLoginResultTests
    {
        private const string Issuer = "https://someserver";
        private readonly IUser _mockUser = new Mock<IUser>().Object;
        private readonly DateTimeOffset _now = DateTimeOffset.Now;

        [Test]
        public void Success_ShouldCreateExpectedResultWithSuccessCode()
        {            
            var result = AuthenticationServerLoginResult.Success(_mockUser, Issuer, _now);
            result.Result.Code.Should().Be(LoginResultCode.Success);
        }

        [Test]
        public void Success_ShouldSetCorrectIssuer()
        {            
            var result = AuthenticationServerLoginResult.Success(_mockUser, Issuer, _now);
            result.Issuer.Should().Be(Issuer);
        }

        [Test]
        public void Success_ShouldSetCorrectAuthenticationTime()
        {            
            var result = AuthenticationServerLoginResult.Success(_mockUser, Issuer, _now);
            result.AuthenticationTime.Should().Be(_now);
        }

        [Test]
        public void Failed_ValidFailedCode_ShouldSetCorrectCode()
        {
            var result = AuthenticationServerLoginResult.Failed(LoginResultCode.ComputerNameNotSet);
            result.Result.Code.Should().Be(LoginResultCode.ComputerNameNotSet);
        }

        [Test]
        public void Failed_SuccessCode_ShouldThrowException()
        {
            Action failed = () => AuthenticationServerLoginResult.Failed(LoginResultCode.Success);
            failed.ShouldThrow<ArgumentException>();
        }
    }
}
