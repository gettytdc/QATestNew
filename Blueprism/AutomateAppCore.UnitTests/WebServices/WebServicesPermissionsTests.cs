#if UNITTESTS
using System;
using System.Collections.Generic;
using System.Linq;
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateProcessCore.Processes;
using BluePrism.Utilities.Functional;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.WebServices
{
    [TestFixture]
    public class WebServicesPermissionsTests : UnitTestBase<WebServicesPermissions>
    {
        private static readonly Permission InvalidPermission =
            Permission.CreatePermission(123, "Invalid");

        private static readonly Dictionary<int, Permission> Permissions =
            new[] {"Test", "Test2", "Test3"}.Map(AssignIndexes)
                .Select(x => Permission.CreatePermission(x.index, x.value))
                .Concat(new[] {InvalidPermission}).ToDictionary(k => k.Id, v => v);

        private static readonly Dictionary<int, PermissionGroup> Groups =
            new[] {"Test", "Test2", "Test3"}.Map(AssignIndexes)
                .Select(x => new PermissionGroup(x.index, x.value)).ToDictionary(k => k.Id, v => v);

        private static IEnumerable<(int index, T value)> AssignIndexes<T>(IEnumerable<T> items) =>
            items.Map(x => (x, Enumerable.Range(0, x.Count())))
                .Map(x => x.Item2.Zip(x.Item1, (i1, i2) => (i1, i2)));

        protected static IEnumerable<(DiagramType, bool)> TestCaseGenerator() =>
            new[]
            {
                (DiagramType.Process, false), (DiagramType.Process, true), (DiagramType.Object, false),
                (DiagramType.Object, true)
            };

        [Test]
        [TestCaseSource(nameof(TestCaseGenerator))]
        public void CanExecuteWebServiceReturnsExpectedResult(
            (DiagramType processType, bool hasPermission) parameters)
        {
            var roles = new RoleSet {new Role("Test") {Id = 0}};
            var userId = Guid.NewGuid();
            var userMock = GetMock<IUser>();
            userMock.Setup(m => m.HasPermission(It.IsAny<ICollection<Permission>>()))
                .Returns(true);
            userMock.SetupGet(m => m.Roles).Returns(roles);
            userMock.SetupGet(m => m.Id).Returns(userId);
            var expectedPermission = parameters.processType == DiagramType.Process
                ? Permission.ProcessStudio.ExecuteProcessAsWebService
                : Permission.ObjectStudio.ExecuteBusinessObjectAsWebService;
            var mockMemberPermissions = GetMock<IMemberPermissions>();
            mockMemberPermissions.Setup(m => m.HasPermission(userMock.Object, expectedPermission))
                .Returns(parameters.hasPermission);
            var processId = Guid.NewGuid();
            var mockPermissionData = new PermissionData(Permissions, Groups);
            var serverMock = GetMock<IServer>();
            serverMock.Setup(m => m.GetPermissionData())
                .Returns(mockPermissionData);
            serverMock.Setup(m =>
                m.GetEffectiveMemberPermissionsForProcess(processId)).Returns(mockMemberPermissions.Object);
            Permission.Init(serverMock.Object);
            var (success, errorMessage) = ClassUnderTest.CanExecuteWebService(parameters.processType, userMock.Object, processId);
            success.Should().Be(parameters.hasPermission);
            if (parameters.hasPermission)
            {
                errorMessage.Should().BeEmpty();
            }
            else
            {
                errorMessage.Should().NotBeEmpty();
            }
        }
    }
}
#endif
