#if UNITTESTS
using BluePrism.AutomateAppCore;
using BluePrism.UnitTesting;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using BluePrism.UnitTesting.TestSupport;

namespace AutomateAppCore.UnitTests.AuditWorkQueue
{
    [TestFixture]
    public class AuditWorkQueueTests
    {
        [Test]
        public void clsServerWorkQueues_WorkQueueClearWorked_ReturnsZeroOnEmptyQueueItemList()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            var permissionValidatorMock = new Mock<IPermissionValidator>();

            var testServer = new clsServer();
            var emptyId = Guid.Empty;
            var selectedQueueItems = new List<clsWorkQueueItem>();

            ReflectionHelper.SetPrivateField("mPermissionValidator", testServer, permissionValidatorMock.Object);

            Assert.IsTrue(testServer.WorkQueueClearWorked(emptyId, selectedQueueItems, "", true).Equals(0));
        }

        [Test]
        public void clsServerWorkQueues_WorkQueueDefer_ReturnsEmptyListOnEmptyQueueItemList()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            var permissionValidatorMock = new Mock<IPermissionValidator>();

            var testServer = new clsServer();
            var emptyId = Guid.Empty;
            var selectedQueueItems = new List<clsWorkQueueItem>();
            var emptyDate = new DateTime();

            ReflectionHelper.SetPrivateField("mPermissionValidator", testServer, permissionValidatorMock.Object);

            Assert.DoesNotThrow(() => testServer.WorkQueueDefer(selectedQueueItems, emptyDate, emptyId, true));
        }

        [Test]
        public void clsServerWorkQueues_WorkQueueRetryItems_ReturnsEmptyListOnEmptyQueueItemList()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            var permissionValidatorMock = new Mock<IPermissionValidator>();

            var testServer = new clsServer();
            var emptyId = Guid.Empty;
            var selectedQueueItems = new List<clsWorkQueueItem>();

            ReflectionHelper.SetPrivateField("mPermissionValidator", testServer, permissionValidatorMock.Object);

            var stringCollection = testServer.WorkQueueForceRetry(selectedQueueItems, emptyId, true);

            Assert.IsTrue(stringCollection.Count.Equals(0));
        }

        [Test]
        public void clsServerWorkQueues_GetMultipleIdsComments_MoreThan20ItemsIDs()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            var testServer = new clsServer();
            const string commentResource = "{0}, {1}, {2}";
            var guidList = new List<Guid>{Guid.Parse("0608a9e8-ec4d-406e-9818-c4d73b00917f"),
                Guid.Parse("232fa9a0-fbdb-4742-a94e-972bc21455de"),
                Guid.Parse("20cc0a2c-91cf-4eca-a19c-ab7129d016bd"),
                Guid.Parse("ba5b5a00-1ed5-48a8-a5f1-b26d76063d76"),
                Guid.Parse("f476c3a8-8812-4091-849b-820be7a5f830"),
                Guid.Parse("208eefa2-2230-4f1f-89dc-07d5d27f7646"),
                Guid.Parse("723ceadd-6eaa-4b0d-9f2f-0321610b24ef"),
                Guid.Parse("48dd27a2-ebe5-4908-b132-d904d9d74547"),
                Guid.Parse("4773a803-87b4-4ff8-96b5-808ecf42f8cc"),
                Guid.Parse("e691e7e7-799f-44b6-a14e-7ea4746fafee"),
                Guid.Parse("3968477e-95e6-4bc3-b87d-c8cc36518823"),
                Guid.Parse("adb86c21-300a-413e-a3f3-c2426fa917df"),
                Guid.Parse("a8568105-fd15-4566-9e20-a1f562d1192f"),
                Guid.Parse("e6665bf6-8dff-4d76-9199-c74c9ae42577"),
                Guid.Parse("1a992042-d69a-4989-a097-3499fb01b051"),
                Guid.Parse("2fc0b870-50ca-4e97-a66b-d23915dacbb2"),
                Guid.Parse("037600d7-7bc6-463c-a92a-c13e0d2cf37d"),
                Guid.Parse("0a8531fb-6c3f-4a06-95f3-494e2d1fccdd"),
                Guid.Parse("08e25e85-6345-4e72-b591-6efdf3e40330"),
                Guid.Parse("9a7ae9f2-0c26-41b8-9c17-e9963b147c53"),
                Guid.Parse("35b4dfb0-5459-450a-8728-c660a76f2da0")};
            var queueId = Guid.Parse("9a7ae9f2-0c26-41b8-9c17-e9963b147c53");

            var result = (List<string>)ReflectionHelper.InvokePrivateMethod("GetMultipleIdsComments", testServer,
                commentResource, guidList, queueId);
            
            Assert.IsTrue(result[0] == "20, 9a7ae9f2-0c26-41b8-9c17-e9963b147c53, 0608a9e8-ec4d-406e-9818-c4d73b00917f, 232fa9a0-fbdb-4742-a94e-972bc21455de, " +
                          "20cc0a2c-91cf-4eca-a19c-ab7129d016bd, ba5b5a00-1ed5-48a8-a5f1-b26d76063d76, f476c3a8-8812-4091-849b-820be7a5f830, 208eefa2-2230-4f1f-89dc-07d5d27f7646," +
                          " 723ceadd-6eaa-4b0d-9f2f-0321610b24ef, 48dd27a2-ebe5-4908-b132-d904d9d74547, 4773a803-87b4-4ff8-96b5-808ecf42f8cc, e691e7e7-799f-44b6-a14e-7ea4746fafee, " +
                          "3968477e-95e6-4bc3-b87d-c8cc36518823, adb86c21-300a-413e-a3f3-c2426fa917df, a8568105-fd15-4566-9e20-a1f562d1192f, e6665bf6-8dff-4d76-9199-c74c9ae42577," +
                          " 1a992042-d69a-4989-a097-3499fb01b051, 2fc0b870-50ca-4e97-a66b-d23915dacbb2, 037600d7-7bc6-463c-a92a-c13e0d2cf37d, 0a8531fb-6c3f-4a06-95f3-494e2d1fccdd, " +
                          "08e25e85-6345-4e72-b591-6efdf3e40330, 9a7ae9f2-0c26-41b8-9c17-e9963b147c53");
            Assert.IsTrue(result[1] == "1, 9a7ae9f2-0c26-41b8-9c17-e9963b147c53, 35b4dfb0-5459-450a-8728-c660a76f2da0");
            Assert.IsTrue(result.Count == 2);
        }

        [Test]
        public void clsServerWorkQueues_GetMultipleIdsComments_LessThan20ItemsIDs()
        {
            LegacyUnitTestHelper.SetupDependencyResolver();
            var testServer = new clsServer();
            const string commentResource = "{0}, {1}, {2}";
            var guidList = new List<Guid>{Guid.Parse("0608a9e8-ec4d-406e-9818-c4d73b00917f"),
                Guid.Parse("232fa9a0-fbdb-4742-a94e-972bc21455de"),
                Guid.Parse("20cc0a2c-91cf-4eca-a19c-ab7129d016bd")};
                var queueId = Guid.Parse("9a7ae9f2-0c26-41b8-9c17-e9963b147c53");

            var result = (List<string>)ReflectionHelper.InvokePrivateMethod("GetMultipleIdsComments", testServer,
                commentResource, guidList, queueId);

            Assert.IsTrue(result[0] == "3, 9a7ae9f2-0c26-41b8-9c17-e9963b147c53, 0608a9e8-ec4d-406e-9818-c4d73b00917f, 232fa9a0-fbdb-4742-a94e-972bc21455de, " +
                                               "20cc0a2c-91cf-4eca-a19c-ab7129d016bd");
            Assert.IsTrue(result.Count == 1);
        }
    }
}
#endif
