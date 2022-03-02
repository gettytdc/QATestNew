using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Commands;
using BluePrism.AutomateAppCore;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using BluePrism.Server.Domain.Models;

namespace AutomateAppCore.UnitTests.Commands
{
    [TestFixture]
    public class CommandTests : UnitTestBase<CommandBase>
    {
        private static readonly IEnumerable<(Type, CommandAuthenticationMode, string)> TestClasses =
        new List<(Type, CommandAuthenticationMode, string)>
        {
            (typeof(ActionCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(AvailabilityCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(BusyCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(CapsCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(ConnectionsCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(ControllerCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(CreateAsCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(CreateCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(DeleteAsCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(DeleteCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(GetParamsCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(InternalAuthCommand), CommandAuthenticationMode.Any, string.Empty),
            (typeof(MembersCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(OutputsCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(PasswordCommand), CommandAuthenticationMode.Any, string.Empty),
            (typeof(PingCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(PoolCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(ProcListCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(ProxyForCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(QuitCommand), CommandAuthenticationMode.Any, string.Empty),
            (typeof(SetVarCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(ShutdownCommand), CommandAuthenticationMode.AuthedOrLocal,Permission.Resources.ControlResource),
            (typeof(StartAsCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(StartCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(StartPCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(StatusCommand), CommandAuthenticationMode.AuthedOrLocal, string.Empty),
            (typeof(StopCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(TokenCommand), CommandAuthenticationMode.Any, string.Empty),
            (typeof(UserCommand), CommandAuthenticationMode.Any, string.Empty),
            (typeof(UserListCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(VarsCommand), CommandAuthenticationMode.Authed, string.Empty),
            (typeof(VarUpdatesCommand), CommandAuthenticationMode.Authed, Permission.Resources.ControlResource),
            (typeof(VersionCommand), CommandAuthenticationMode.Any, string.Empty)
        };

        [Test]
        [TestCaseSource(nameof(TestCaseGenerator))]
        public void TestCheckPermissions(TestCaseParameter parameters)
        {
            var memberPermissionsMock = GetMock<IMemberPermissions>();
            memberPermissionsMock.
                Setup(m => m.HasPermission(It.IsAny<IUser>(), It.Is<string[]>(x => x.Contains(parameters.ExpectedPermissionCheck)))).Returns(parameters.HasPermission);
            var userMock = GetMock<IUser>();
            if (parameters.IsSystemUser)
            {
                userMock.Setup(x => x.AuthType).Returns(AuthMode.System);
            }
            else
            {
                userMock.Setup(x => x.AuthType).Returns(AuthMode.Native);
            }

            var classUnderTest =
                (ICommand)
                parameters.CommandType
                    .GetConstructor(new[]
                    {
                        typeof(IListenerClient),
                        typeof(IListener),
                        typeof(IServer),
                        typeof(Func<IGroupPermissions, IMemberPermissions>)
                    })
                    ?.Invoke(new object[]
                    {
                        GetMock<IListenerClient>().Object,
                        GetMock<IListener>().Object,
                        GetMock<IServer>().Object,
                        (Func<IGroupPermissions, IMemberPermissions>)((IGroupPermissions _) => memberPermissionsMock.Object)
                    });

            if (!string.IsNullOrEmpty(parameters.ExpectedPermissionCheck) && classUnderTest != null)
            {
                var result = classUnderTest.CheckPermissions(userMock.Object, GetResourceId());
                result.allowed.Should().Be(parameters.ExpectedOutcome);

            }

            Assert.That(classUnderTest != null &&
                        classUnderTest.CommandAuthenticationRequired == parameters.ExpectedAuthenticationMode);
        }
        protected static IEnumerable<TestCaseParameter> TestCaseGenerator()
        {
            var list = new List<(bool, bool, bool)>()
                {(true, true, true), (true, false, true), (false, true, true), (false, false, false)};

            return list.SelectMany(x =>
                TestClasses.Select(z => new TestCaseParameter
                {
                    IsSystemUser = x.Item1,
                    HasPermission = x.Item2,
                    ExpectedOutcome = x.Item3,
                    ExpectedPermissionCheck = z.Item3,
                    ExpectedAuthenticationMode = z.Item2,
                    CommandType = z.Item1
                }));
        }

        private static Guid GetResourceId()
        {
            return Guid.NewGuid();
        }

        public class TestCaseParameter
        {
            public Type CommandType;
            public CommandAuthenticationMode ExpectedAuthenticationMode;
            public bool ExpectedOutcome;
            public string ExpectedPermissionCheck;
            public bool HasPermission;
            public bool IsSystemUser;
        }
    }
}
