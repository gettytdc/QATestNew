namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Drawing;
    using AutoFixture;
    using AutomateAppCore;
    using AutomateProcessCore;
    using BluePrism.Server.Domain.Models;
    using Common.Security;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;
    using Func;
    using static DataHelper;
    using QueueStatus = Domain.QueueStatus;

    public static class WorkQueuesHelper
    {
        public static void ValidateModelsAreEqual(clsWorkQueueItem[] bluePrism, WorkQueueItemNoDataXml[] domain)
        {
            bluePrism.Should().HaveCount(domain.Length);

            for (var i = 0; i < bluePrism.Length; i++)
                ValidateModelsAreEqual(bluePrism[i], domain[i]);
        }

        public static void ValidateModelsAreEqual(clsWorkQueueItem bluePrism, WorkQueueItemNoDataXml domain)
        {
            bluePrism.ID.Should().Be(domain.Id);
            bluePrism.Priority.Should().Be(domain.Priority);
            bluePrism.Ident.Should().Be(domain.Ident);
            StatesAreEqual(bluePrism.CurrentState, domain.State).Should().BeTrue();
            bluePrism.KeyValue.Should().Be(domain.KeyValue);
            bluePrism.Status.Should().Be(domain.Status);
            bluePrism.Tags.Should().BeEquivalentTo(domain.Tags);
            bluePrism.Attempt.Should().Be(domain.AttemptNumber);
            bluePrism.Loaded.Should().Be(domain.LoadedDate is Some<DateTimeOffset> loadedDate ? loadedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Deferred.Should().Be(domain.DeferredDate is Some<DateTimeOffset> deferredDate ? deferredDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Locked.Should().Be(domain.LockedDate is Some<DateTimeOffset> lockedDate ? lockedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.CompletedDate.Should().Be(domain.CompletedDate is Some<DateTimeOffset> completedDate ? completedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Worktime.Should().Be(domain.WorkTimeInSeconds);
            bluePrism.AttemptWorkTime.Should().Be(domain.AttemptWorkTimeInSeconds);
            bluePrism.ExceptionDate.Should().Be(domain.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? exceptionedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.LastUpdated.Should().Be(domain.LastUpdated is Some<DateTimeOffset> lastUpdated ? lastUpdated.Value.DateTime : DateTime.MinValue);
            bluePrism.ExceptionReason.Should().Be(domain.ExceptionReason);
            bluePrism.Resource.Should().Be(domain.Resource);
        }

        public static void ValidateModelsAreEqual(clsWorkQueueItem bluePrism, WorkQueueItem domain)
        {
            bluePrism.ID.Should().Be(domain.Id);
            bluePrism.Priority.Should().Be(domain.Priority);
            bluePrism.Ident.Should().Be(domain.Ident);
            StatesAreEqual(bluePrism.CurrentState, domain.State).Should().BeTrue();
            bluePrism.KeyValue.Should().Be(domain.KeyValue);
            bluePrism.Status.Should().Be(domain.Status);
            bluePrism.Tags.Should().BeEquivalentTo(domain.Tags);
            bluePrism.Attempt.Should().Be(domain.AttemptNumber);
            bluePrism.Loaded.Should().Be(domain.LoadedDate is Some<DateTimeOffset> loadedDate ? loadedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Deferred.Should().Be(domain.DeferredDate is Some<DateTimeOffset> deferredDate ? deferredDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Locked.Should().Be(domain.LockedDate is Some<DateTimeOffset> lockedDate ? lockedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.CompletedDate.Should().Be(domain.CompletedDate is Some<DateTimeOffset> completedDate ? completedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.Worktime.Should().Be(domain.WorkTimeInSeconds);
            bluePrism.AttemptWorkTime.Should().Be(domain.AttemptWorkTimeInSeconds);
            bluePrism.ExceptionDate.Should().Be(domain.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? exceptionedDate.Value.DateTime : DateTime.MinValue);
            bluePrism.LastUpdated.Should().Be(domain.LastUpdated is Some<DateTimeOffset> lastUpdated ? lastUpdated.Value.DateTime : DateTime.MinValue);
            bluePrism.ExceptionReason.Should().Be(domain.ExceptionReason);
            bluePrism.Resource.Should().Be(domain.Resource);
            ValidateCollectionsAreEqual(bluePrism.Data, domain.Data);
        }

        public static void ValidateModelsAreEqual(clsWorkQueueItem[] bluePrism, WorkQueueItem[] domain)
        {
            bluePrism.Should().HaveCount(domain.Length);

            for (var i = 0; i < bluePrism.Length; i++)
                ValidateModelsAreEqual(bluePrism[i], domain[i]);
        }

        public static void ValidateModelsAreEqual(clsWorkQueue clsWorkQueue, WorkQueue domainWorkQueue)
        {
            clsWorkQueue.Id.Should().Be(domainWorkQueue.Id);
            clsWorkQueue.Name.Should().Be(domainWorkQueue.Name);
            clsWorkQueue.KeyField.Should().Be(domainWorkQueue.KeyField);
            clsWorkQueue.MaxAttempts.Should().Be(domainWorkQueue.MaxAttempts);
            clsWorkQueue.IsRunning.Should().Be(domainWorkQueue.Status == QueueStatus.Running);
            clsWorkQueue.IsEncrypted.Should().Be(domainWorkQueue.IsEncrypted);
            clsWorkQueue.EncryptionKeyID.Should().Be(domainWorkQueue.EncryptionKeyId);
            clsWorkQueue.Pending.Should().Be(domainWorkQueue.PendingItemCount);
            clsWorkQueue.Completed.Should().Be(domainWorkQueue.CompletedItemCount);
            clsWorkQueue.Locked.Should().Be(domainWorkQueue.LockedItemCount);
            clsWorkQueue.Exceptioned.Should().Be(domainWorkQueue.ExceptionedItemCount);
            clsWorkQueue.AverageWorkedTime.Should().Be(domainWorkQueue.AverageWorkTime);
            clsWorkQueue.TotalWorkTime.Should().Be(domainWorkQueue.TotalCaseDuration);
            clsWorkQueue.ProcessId.Should().Be(domainWorkQueue.ProcessId);
            clsWorkQueue.ResourceGroupId.Should().Be(domainWorkQueue.ResourceGroupId);
            clsWorkQueue.TargetSessionCount.Should().Be(domainWorkQueue.TargetSessionCount);
            clsWorkQueue.TotalAttempts.Should().Be(domainWorkQueue.TotalItemCount);
        }

        public static void ValidateModelsAreEqual(WorkQueueWithGroup workQueueWithGroup, WorkQueue domainWorkQueue)
        {
            workQueueWithGroup.Id.Should().Be(domainWorkQueue.Id);
            workQueueWithGroup.Name.Should().Be(domainWorkQueue.Name);
            workQueueWithGroup.KeyField.Should().Be(domainWorkQueue.KeyField);
            workQueueWithGroup.MaxAttempts.Should().Be(domainWorkQueue.MaxAttempts);
            workQueueWithGroup.IsRunning.Should().Be(domainWorkQueue.Status == QueueStatus.Running);
            workQueueWithGroup.IsEncrypted.Should().Be(domainWorkQueue.IsEncrypted);
            workQueueWithGroup.EncryptionKeyId.Should().Be(domainWorkQueue.EncryptionKeyId);
            workQueueWithGroup.PendingItemCount.Should().Be(domainWorkQueue.PendingItemCount);
            workQueueWithGroup.CompletedItemCount.Should().Be(domainWorkQueue.CompletedItemCount);
            workQueueWithGroup.LockedItemCount.Should().Be(domainWorkQueue.LockedItemCount);
            workQueueWithGroup.ExceptionedItemCount.Should().Be(domainWorkQueue.ExceptionedItemCount);
            workQueueWithGroup.AverageWorkTime.Should().Be(domainWorkQueue.AverageWorkTime);
            workQueueWithGroup.TotalCaseDuration.Should().Be(domainWorkQueue.TotalCaseDuration);
            workQueueWithGroup.ProcessId.Should().Be(domainWorkQueue.ProcessId);
            workQueueWithGroup.ResourceGroupId.Should().Be(domainWorkQueue.ResourceGroupId);
            workQueueWithGroup.TargetSessionCount.Should().Be(domainWorkQueue.TargetSessionCount);
            workQueueWithGroup.TotalItemCount.Should().Be(domainWorkQueue.TotalItemCount);
            workQueueWithGroup.GroupName.Should().Be(domainWorkQueue.GroupName);
            workQueueWithGroup.GroupId.Should().Be(domainWorkQueue.GroupId);
        }

        public static void ValidateParametersModelsAreEqual(Server.Domain.Models.WorkQueueParameters serverWorkQueueParameters, Domain.WorkQueueParameters domainWorkQueueParameters)
        {
            serverWorkQueueParameters.ItemsPerPage.Should().Be(domainWorkQueueParameters.ItemsPerPage);
            serverWorkQueueParameters.SortBy.ToString().Should().Be(domainWorkQueueParameters.SortBy.ToString());
        }

        public static bool StatesAreEqual(clsWorkQueueItem.State bluePrismState, WorkQueueItemState domainState) =>
            (bluePrismState == clsWorkQueueItem.State.Locked && domainState == WorkQueueItemState.Locked)
            || (bluePrismState == clsWorkQueueItem.State.Deferred && domainState == WorkQueueItemState.Deferred)
            || (bluePrismState == clsWorkQueueItem.State.Completed && domainState == WorkQueueItemState.Completed)
            || (bluePrismState == clsWorkQueueItem.State.Exceptioned && domainState == WorkQueueItemState.Exceptioned)
            || (bluePrismState == clsWorkQueueItem.State.None && domainState == WorkQueueItemState.None)
            || (bluePrismState == clsWorkQueueItem.State.Pending && domainState == WorkQueueItemState.Pending);

        public static WorkQueue GetQueueWithName(string name) => new Fixture()
               .Build<WorkQueue>()
               .With(x => x.Name, name)
               .With(x => x.IsEncrypted, true)
               .With(x => x.EncryptionKeyId, 10)
               .Create();

        public static clsWorkQueueItem GetTestBluePrismWorkQueueItem(Guid id, DateTime baseDate, int priority = 5, int attempt = 9)
        {
            var childCollection = new clsCollection();
            childCollection.Add(new clsCollectionRow
            {
                { "Number", new clsProcessValue(123) },
                { "Text", new clsProcessValue("Test") },
            });

            var data = new clsCollection();
            data.Add(new clsCollectionRow
            {
                {"Binary", new clsProcessValue(new byte[] {0x01, 0x02, 0x03})},
                {"Collection", new clsProcessValue(childCollection)},
                {"Date", new clsProcessValue(DataType.date, baseDate)},
                {"DateTime", new clsProcessValue(DataType.datetime, baseDate)},
                {"Bitmap", new clsProcessValue(new Bitmap(10, 10))},
                {"Password", new clsProcessValue(new SafeString("Password"))},
                {"TimeSpan", new clsProcessValue(TimeSpan.FromSeconds(66))},
                {"Time", new clsProcessValue(DataType.time, "12:34:56")},
                {"Flag", new clsProcessValue(true)},
                {"Decimal", new clsProcessValue(12.34M)},
                {"Double", new clsProcessValue(43.21)},
                {"Integer", new clsProcessValue(987)},

            });
            data.Add(new clsCollectionRow { { "Test", new clsProcessValue(42) } });

            var item = new clsWorkQueueItem(id, 123, "Test")
            {
                Priority = priority,
                CurrentState = clsWorkQueueItem.State.Locked,
                Status = "Test Status",
                Attempt = attempt,
                Loaded = baseDate.AddMinutes(-10),
                Deferred = baseDate.AddMinutes(-9),
                Locked = baseDate.AddMinutes(-8),
                CompletedDate = baseDate.AddMinutes(-7),
                Worktime = 222,
                AttemptWorkTime = 333,
                ExceptionDate = baseDate.AddMinutes(-6),
                LastUpdated =  baseDate.AddMinutes(-2),
                ExceptionReason = "TestExceptionReason",
                Data = data
            };
            item.AddTag("Tag1");
            item.AddTag("Tag2");

            return item;
        }

        //TODO this is required to get an integration test to pass, ultimately we would like to use the method GetTestBluePrismWorkQueueItem
        public static clsWorkQueueItem GetTestBluePrismWorkQueueItemLimitedCollection(Guid id, DateTime baseDate, int priority = 5, int attempt = 9)
        {
            var childCollection = new clsCollection();
            childCollection.Add(new clsCollectionRow
            {
                { "Number", new clsProcessValue(123) },
                { "Text", new clsProcessValue("Test") },
            });

            var data = new clsCollection();
            data.Add(new clsCollectionRow
            {
                {"Integer", new clsProcessValue(987)},

            });

            var item = new clsWorkQueueItem(id, 123, "Test")
            {
                Priority = priority,
                CurrentState = clsWorkQueueItem.State.Locked,
                Status = "Test Status",
                Attempt = attempt,
                Loaded = baseDate.AddMinutes(-10),
                Deferred = baseDate.AddMinutes(-9),
                Locked = baseDate.AddMinutes(-8),
                CompletedDate = baseDate.AddMinutes(-7),
                Worktime = 222,
                AttemptWorkTime = 333,
                ExceptionDate = baseDate.AddMinutes(-6),
                LastUpdated = baseDate.AddMinutes(-2),
                ExceptionReason = "TestExceptionReason",
                Data = data
            };
            item.AddTag("Tag1");
            item.AddTag("Tag2");

            return item;
        }

        public static clsWorkQueue GetTestBluePrismClsWorkQueue() =>
            new clsWorkQueue
            {
                Name = "testName",
                AverageWorkedTime = new TimeSpan(2222222),
                ActiveSessionIds = { Guid.NewGuid(), Guid.NewGuid() },
                Completed = 7,
                Deferred = 8,
                EncryptionKeyID = 4,
                Id = Guid.NewGuid(),
                Exceptioned = 2,
                Ident = 3,
                IsAssignedProcessHidden = true,
                IsResourceGroupHidden = true,
                IsRunning = true,
                KeyField = "testKeyField",
                Locked = 3,
                MaxAttempts = 2,
                Pending = 3,
                ProcessId = Guid.NewGuid(),
                ProcessName = "testProcessName",
                ResourceGroupId = Guid.NewGuid(),
                ResourceGroupName = "testResourceGroupName",
                SnapshotConfigurationId = 2,
                TargetSessionCount = 4,
                TotalAttempts = 6,
                TotalWorkTime = new TimeSpan(333333)
            };

        public static WorkQueue GetTestDomainWorkQueue() =>
            new WorkQueue
            {
                Name = "testName",
                Id = Guid.NewGuid(),
                Status = QueueStatus.Running,
                KeyField = "testKeyField",
                MaxAttempts = 2,
                ProcessId = Guid.NewGuid(),
                ResourceGroupId = Guid.NewGuid(),
                TargetSessionCount = 4,
                TotalItemCount = 15,
                AverageWorkTime = new TimeSpan(3333333),
                PendingItemCount = 4,
                CompletedItemCount = 5,
                ExceptionedItemCount = 6,
                LockedItemCount = 7,
                TotalCaseDuration = new TimeSpan(444444),
                EncryptionKeyId = 5,
                IsEncrypted = true,
                GroupName = "test group",
                GroupId = Guid.NewGuid(),
            };

        public static Domain.WorkQueueParameters GetTestDomainWorkQueueParameters() =>
            new Domain.WorkQueueParameters
            {
                SortBy =  Domain.WorkQueueSortByProperty.CompletedAsc,
                ItemsPerPage = 10,
                NameFilter = new NullFilter<string>(),
                AverageWorkTimeFilter = new NullFilter<int>(),
                MaxAttemptsFilter = new NullFilter<int>(),
                CompletedItemCountFilter = new NullFilter<int>(),
                ExceptionedItemCountFilter = new NullFilter<int>(),
                KeyFieldFilter = new NullFilter<string>(),
                LockedItemCountFilter = new NullFilter<int>(),
                PendingItemCountFilter = new NullFilter<int>(),
                QueueStatusFilter = new NullFilter<Domain.QueueStatus>(),
                TotalCaseDurationFilter = new NullFilter<int>(),
                TotalItemCountFilter = new NullFilter<int>()
            };

        public static WorkQueueWithGroup GetTestWorkQueueWithGroup() =>
            new WorkQueueWithGroup
            {
                Id = Guid.NewGuid(),
                Name = "test",
                IsRunning = true,
                KeyField = "test field",
                MaxAttempts = 2,
                EncryptionKeyId = 5,
                PendingItemCount = 10,
                CompletedItemCount = 5,
                ExceptionedItemCount = 5,
                LockedItemCount = 0,
                TotalItemCount = 20,
                AverageWorkTime = new TimeSpan(2222222),
                TotalCaseDuration = new TimeSpan(2222222),
                ProcessId = Guid.NewGuid(),
                ResourceGroupId = Guid.NewGuid(),
                TargetSessionCount = 10,
                GroupName = "test group",
                GroupId = Guid.NewGuid(),
            };
    }
}
