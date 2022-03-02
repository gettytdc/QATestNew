namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Linq;
    using AutomateAppCore;
    using BluePrism.Server.Domain.Models;
    using static Func.OptionHelper;
    using QueueStatus = Domain.QueueStatus;

    public static class WorkQueueMapper
    {
        public static clsWorkQueue ToBluePrismObject(this Domain.WorkQueue workQueue) =>
            new clsWorkQueue
            {
                Id = workQueue.Id,
                Name = workQueue.Name,
                KeyField = workQueue.KeyField,
                MaxAttempts = workQueue.MaxAttempts,
                IsRunning = workQueue.Status == QueueStatus.Running,
                EncryptionKeyID = workQueue.EncryptionKeyId,
                Pending = workQueue.PendingItemCount,
                Completed = workQueue.CompletedItemCount,
                Locked = workQueue.LockedItemCount,
                Exceptioned = workQueue.ExceptionedItemCount,
                AverageWorkedTime = workQueue.AverageWorkTime,
                TotalWorkTime = workQueue.TotalCaseDuration,
                ProcessId = workQueue.ProcessId,
                ResourceGroupId = workQueue.ResourceGroupId,
                TargetSessionCount = workQueue.TargetSessionCount,
                TotalAttempts = workQueue.TotalItemCount,
            };

        public static WorkQueueWithGroup ToBluePrismWorkQueueWithGroup(this Domain.WorkQueue workQueue) =>
            new WorkQueueWithGroup
            {
                Id = workQueue.Id,
                Ident = workQueue.Ident,
                Name = workQueue.Name ?? string.Empty,
                KeyField = workQueue.KeyField ?? string.Empty,
                MaxAttempts = workQueue.MaxAttempts,
                IsRunning = workQueue.Status == QueueStatus.Running,
                EncryptionKeyId = workQueue.EncryptionKeyId,
                PendingItemCount = workQueue.PendingItemCount,
                CompletedItemCount = workQueue.CompletedItemCount,
                LockedItemCount = workQueue.LockedItemCount,
                ExceptionedItemCount = workQueue.ExceptionedItemCount,
                AverageWorkTime = workQueue.AverageWorkTime,
                TotalCaseDuration = workQueue.TotalCaseDuration,
                ProcessId = workQueue.ProcessId,
                ResourceGroupId = workQueue.ResourceGroupId,
                TargetSessionCount = workQueue.TargetSessionCount,
                TotalItemCount = workQueue.TotalItemCount,
                GroupName = workQueue.GroupName ?? string.Empty,
                GroupId = workQueue.GroupId,
            };

        public static Domain.WorkQueue ToDomainObject(this clsWorkQueue workQueue) =>
            new Domain.WorkQueue
            {
                Id = workQueue.Id,
                Ident = workQueue.Ident,
                Name = workQueue.Name,
                KeyField = workQueue.KeyField,
                MaxAttempts = workQueue.MaxAttempts,
                Status = workQueue.IsRunning ? QueueStatus.Running : QueueStatus.Paused,
                EncryptionKeyId = workQueue.EncryptionKeyID,
                IsEncrypted = workQueue.IsEncrypted,
                PendingItemCount = workQueue.Pending,
                CompletedItemCount = workQueue.Completed,
                LockedItemCount = workQueue.Locked,
                ExceptionedItemCount = workQueue.Exceptioned,
                AverageWorkTime = workQueue.AverageWorkedTime,
                TotalCaseDuration = workQueue.TotalWorkTime,
                ProcessId = workQueue.ProcessId,
                ResourceGroupId = workQueue.ResourceGroupId,
                TargetSessionCount = workQueue.TargetSessionCount,
                TotalItemCount = workQueue.TotalAttempts,
            };

        public static Domain.WorkQueueItem ToDomainObject(this clsWorkQueueItem workQueueItem) =>
            new Domain.WorkQueueItem
            {
                Id = workQueueItem.ID,
                Ident = workQueueItem.Ident,
                Priority = workQueueItem.Priority,
                State = (Domain.WorkQueueItemState)workQueueItem.CurrentState,
                KeyValue = workQueueItem.KeyValue,
                Status = workQueueItem.Status,
                Tags = workQueueItem.Tags.ToArray(),
                AttemptNumber = workQueueItem.Attempt,
                LoadedDate = workQueueItem.Loaded == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Loaded, TimeSpan.Zero)),
                DeferredDate = workQueueItem.Deferred == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Deferred, TimeSpan.Zero)),
                LockedDate = workQueueItem.Locked == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Locked, TimeSpan.Zero)),
                CompletedDate = workQueueItem.CompletedDate == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.CompletedDate, TimeSpan.Zero)),
                WorkTimeInSeconds = workQueueItem.Worktime,
                AttemptWorkTimeInSeconds = workQueueItem.AttemptWorkTime,
                ExceptionedDate = workQueueItem.ExceptionDate == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.ExceptionDate, TimeSpan.Zero)),
                LastUpdated = workQueueItem.LastUpdated == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.LastUpdated, TimeSpan.Zero)),
                ExceptionReason = workQueueItem.ExceptionReason,
                Data = workQueueItem.Data.ToDomainObject(),
                Resource = workQueueItem.Resource
            };

        public static Domain.WorkQueue ToDomainObject(this WorkQueueWithGroup workQueue) =>
            new Domain.WorkQueue
            {
                Id = workQueue.Id,
                Ident = workQueue.Ident,
                Name = workQueue.Name ?? string.Empty,
                KeyField = workQueue.KeyField ?? string.Empty,
                MaxAttempts = workQueue.MaxAttempts,
                Status = workQueue.IsRunning ? QueueStatus.Running : QueueStatus.Paused,
                EncryptionKeyId = workQueue.EncryptionKeyId,
                IsEncrypted = workQueue.IsEncrypted,
                PendingItemCount = workQueue.PendingItemCount,
                CompletedItemCount = workQueue.CompletedItemCount,
                LockedItemCount = workQueue.LockedItemCount,
                ExceptionedItemCount = workQueue.ExceptionedItemCount,
                AverageWorkTime = workQueue.AverageWorkTime,
                TotalCaseDuration = workQueue.TotalCaseDuration,
                ProcessId = workQueue.ProcessId,
                ResourceGroupId = workQueue.ResourceGroupId,
                TargetSessionCount = workQueue.TargetSessionCount,
                TotalItemCount = workQueue.TotalItemCount,
                GroupName = workQueue.GroupName ?? string.Empty,
                GroupId = workQueue.GroupId
            };

        public static Domain.WorkQueueItemNoDataXml ToDomainObjectNoDataXml(this clsWorkQueueItem workQueueItem) =>
            new Domain.WorkQueueItemNoDataXml
            {
                Id = workQueueItem.ID,
                Ident = workQueueItem.Ident,
                Priority = workQueueItem.Priority,
                State = (Domain.WorkQueueItemState)workQueueItem.CurrentState,
                KeyValue = workQueueItem.KeyValue,
                Status = workQueueItem.Status,
                Tags = workQueueItem.Tags.ToArray(),
                AttemptNumber = workQueueItem.Attempt,
                LoadedDate = workQueueItem.Loaded == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Loaded, TimeSpan.Zero)),
                DeferredDate = workQueueItem.Deferred == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Deferred, TimeSpan.Zero)),
                LockedDate = workQueueItem.Locked == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.Locked, TimeSpan.Zero)),
                CompletedDate = workQueueItem.CompletedDate == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.CompletedDate, TimeSpan.Zero)),
                WorkTimeInSeconds = workQueueItem.Worktime,
                AttemptWorkTimeInSeconds = workQueueItem.AttemptWorkTime,
                ExceptionedDate = workQueueItem.ExceptionDate == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.ExceptionDate, TimeSpan.Zero)),
                LastUpdated = workQueueItem.LastUpdated == DateTime.MinValue ? None<DateTimeOffset>() : Some(new DateTimeOffset(workQueueItem.LastUpdated, TimeSpan.Zero)),
                ExceptionReason = workQueueItem.ExceptionReason,
                Resource = workQueueItem.Resource
            };
    }
}
