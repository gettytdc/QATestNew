#if UNITTESTS
namespace BluePrism.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using AutomateAppCore;
    using AutomateAppCore.Auth;
    using BluePrism.AutomateAppCore.clsServerPartialClasses.Scheduler;
    using BluePrism.Server.Domain.Models;
    using BluePrism.UnitTesting.TestSupport;
    using Data;
    using Moq;
    using NUnit.Framework;
    using BluePrism.Utilities.Testing;
    using BluePrism.UnitTesting;

    [TestFixture]
    public class ServerSecurityTests : UnitTestBase<clsServer>
    {

        [Test]
        public void SaveScheduleThrowsExceptionOnUserNotLoggedIn()
        {
            var schedule = new SessionRunnerSchedule(null)
            {
                Id = 0
            };

            var scheduleStore = new DatabaseBackedScheduleStore(ClassUnderTest);

            try
            {
                scheduleStore.SaveSchedule(schedule);
                Assert.Fail("Expected exception not thrown.");
            }
            catch (PermissionException)
            {
            }
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void SaveScheduleThrowsExceptionOnInsufficientPermissions(int id)
        {
            var userMock = GetMock<IUser>();
            userMock
                .Setup(m => m.HasPermission(It.IsAny<string[]>()))
                .Returns(false);
            ReflectionHelper.SetPrivateField("mLoggedInUser", ClassUnderTest, userMock.Object);

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", ClassUnderTest, (Func<IDatabaseConnection>)(() => databaseConnectionMock.Object));

            var schedule = new SessionRunnerSchedule(null)
            {
                Id = id
            };

            var scheduleStore = new DatabaseBackedScheduleStore(ClassUnderTest);
            try
            {
                scheduleStore.SaveSchedule(schedule);
                Assert.Fail("Expected exception not thrown.");
            }
            catch (PermissionException)
            {
            }
        }

        [Test]
        [TestCase(0, Permission.Scheduler.CreateSchedule)]
        [TestCase(1, Permission.Scheduler.EditSchedule)]
        public void SaveScheduleContinuesOnCorrectPermissions(int scheduleId, string userPermission)
        {
            var userMock = GetMock<IUser>();
            userMock
                .Setup(m => m.HasPermission(It.Is<string[]>(x => x.Any() && x[0].Equals(userPermission))))
                .Returns(true);
            ReflectionHelper.SetPrivateField("mLoggedInUser", ClassUnderTest, userMock.Object);
            
            var schedule = new SessionRunnerSchedule(null)
            {
                Id = scheduleId
            };

            var dataReaderMock = GetMock<IDataReader>();
            dataReaderMock.Setup(x => x.Read()).Returns(true);

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", ClassUnderTest, (Func<IDatabaseConnection>)(() => databaseConnectionMock.Object));
            
            ReflectionHelper.SetPrivateField("mSchedulerGetSchedule", ClassUnderTest, (Func<IDatabaseConnection,int, SessionRunnerSchedule>)((con,id) => new SessionRunnerSchedule(null)));

            var auditEventGeneratorFactoryMock = GetMock<IModifiedScheduleAuditEventGenerator>();
            auditEventGeneratorFactoryMock
                    .Setup(x => x.Generate())
                    .Returns(Enumerable.Empty<ScheduleAuditEvent>);

            ReflectionHelper.SetPrivateField("mModifiedScheduleAuditEventGeneratorFactory", ClassUnderTest, 
                (Func<SessionRunnerSchedule, SessionRunnerSchedule, IUser, IModifiedScheduleAuditEventGenerator>)((x, y, z) => auditEventGeneratorFactoryMock.Object));

            var scheduleStore = new DatabaseBackedScheduleStore(ClassUnderTest);
            scheduleStore.SaveSchedule(schedule);

            databaseConnectionMock
                .Verify(m => m.ExecuteReturnScalar(It.Is<SqlCommand>(x => x.CommandText.Contains("BPASchedule"))));
        }

        [Test]
        [TestCaseSource(nameof(SecuredMethodsDoNotThrowWithCorrectPermissionsTestCaseFactory))]
        public void SecuredMethodsDoNotThrowWithCorrectPermissions(
            (IEnumerable<string> permissions, Action<DatabaseBackedScheduleStore> callMethod) args)
        {
            var (permissions, callMethod) = args;

            var userMock = GetMock<IUser>();
            userMock
                .Setup(m => m.HasPermission(It.Is<string[]>(x => x.Intersect(permissions).Any())))
                .Returns(true);
            ReflectionHelper.SetPrivateField("mLoggedInUser", ClassUnderTest, userMock.Object);

            var databaseConnectionMock = GetMock<IDatabaseConnection>();
            ReflectionHelper.SetPrivateField("mDatabaseConnectionFactory", ClassUnderTest, (Func<IDatabaseConnection>)(() => databaseConnectionMock.Object));

            var scheduleStore = new DatabaseBackedScheduleStore(ClassUnderTest);
            callMethod(scheduleStore);
        }

        [Test]
        [TestCaseSource(nameof(SecuredMethodsThrowsWithIncorrectPermissionsTestCaseFactory))]
        public void SecuredMethodsThrowsWithIncorrectPermissions(
            (IEnumerable<string> permissions, Action<DatabaseBackedScheduleStore> callMethod) args)
        {
            Assert.Throws<PermissionException>(() => SecuredMethodsDoNotThrowWithCorrectPermissions(args));
        }

        private static IEnumerable<(IEnumerable<string> permissions, Action<DatabaseBackedScheduleStore> callMethod)>
            SecuredMethodsDoNotThrowWithCorrectPermissionsTestCaseFactory()
            =>
                new (IEnumerable<string>, Action<DatabaseBackedScheduleStore>)[]
                {
                    (new[] {Permission.Scheduler.RetireSchedule}, x => x.RetireSchedule(new SessionRunnerSchedule(null))),
                    (new[] {Permission.Scheduler.DeleteSchedule}, x => x.DeleteSchedule(new SessionRunnerSchedule(null) { Id = 1 })),
                    (new[] {Permission.Scheduler.EditSchedule}, x => x.SaveScheduleList(new ScheduleList(null))),
                    (new[] {Permission.Scheduler.EditSchedule}, x => x.SaveScheduleList(new ScheduleList(null) { ID = 1 })),
                    (new[] {Permission.Scheduler.EditSchedule}, x => x.DeleteScheduleList(new ScheduleList(null) { ID = 1 })),
                };

        private static IEnumerable<string> GetAllPermissions() =>
            typeof(Permission)
                .GetNestedTypes()
                .SelectMany(x => x
                    .GetFields()
                    .Where(y => y.FieldType == typeof(string))
                    .Select(y => y.GetValue(null) as string));

        private static IEnumerable<(IEnumerable<string> permissions, Action<DatabaseBackedScheduleStore> callMethod)>
            SecuredMethodsThrowsWithIncorrectPermissionsTestCaseFactory()
            =>
                SecuredMethodsDoNotThrowWithCorrectPermissionsTestCaseFactory()
                    .Select(x => (GetAllPermissions().Except(x.permissions), x.callMethod));

        public override void Setup()
        {
            base.Setup();

            LegacyUnitTestHelper.SetupDependencyResolver();
            /// ClassUnderTest = new clsServer();

            var serverManagerMock = GetMock<ServerManager>();

            serverManagerMock
                .SetupGet(m => m.Server)
                .Returns(ClassUnderTest);

            ReflectionHelper.SetPrivateField<ServerFactory>("mServerManager", null, serverManagerMock.Object);
        }
    }
}
#endif
