namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Func;
    using Models;
    using NUnit.Framework;
    using WorkQueueItemState = Domain.WorkQueueItemState;

    [TestFixture]
    public class WorkQueueItemMapperTests
    {
        [Test]
        public void ToModelNoDataXml_ShouldReturnCorrectlyMappedModels_WhenCalledWithCollection()
        {
            var workQueueItemNoDataXml1 = new WorkQueueItemNoDataXml()
            {
                Id = Guid.NewGuid(),
                Priority = 1,
                Ident = 101,
                State = WorkQueueItemState.Completed,
                KeyValue = "test-key 1",
                Status = "Status 1",
                Tags = new[] { "tag1", "tag2" },
                AttemptNumber = 2,
                LoadedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(1))),
                DeferredDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(2))),
                LockedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(3))),
                CompletedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(4))),
                ExceptionedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(5))),
                LastUpdated = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(6))),
                WorkTimeInSeconds = 10,
                AttemptWorkTimeInSeconds = 20,
                ExceptionReason = "ExceptionReason 1",
                Resource = "ResourceName 1"
            };

            var workQueueItemNoDataXml2 = new WorkQueueItemNoDataXml()
            {
                Id = Guid.NewGuid(),
                Priority = 1,
                Ident = 101,
                State = WorkQueueItemState.Completed,
                KeyValue = "test-key 2",
                Status = "Status 2",
                Tags = new[] { "tag1", "tag2" },
                AttemptNumber = 2,
                LoadedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(1))),
                DeferredDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(2))),
                LockedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(3))),
                CompletedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(4))),
                ExceptionedDate = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(5))),
                LastUpdated = OptionHelper.Some(new DateTimeOffset(DateTime.Now.AddHours(6))),
                WorkTimeInSeconds = 10,
                AttemptWorkTimeInSeconds = 20,
                ExceptionReason = "ExceptionReason 2",
                Resource = "ResourceName 2"
            };

            var workQueueItemsNoDataXml = new List<WorkQueueItemNoDataXml>
            {
                workQueueItemNoDataXml1,
                workQueueItemNoDataXml2
            };

            var results = workQueueItemsNoDataXml.ToModelNoDataXml().ToList();
            results.Count.Should().Be(workQueueItemsNoDataXml.Count);
            results.First().KeyValue.Should().Be(workQueueItemNoDataXml1.KeyValue);
            results.Last().KeyValue.Should().Be(workQueueItemNoDataXml2.KeyValue);
        }


        [Test]
        public void ToModelNoDataXml_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var workQueueItemNoDataXml = new WorkQueueItemNoDataXml()
            {
                Id = Guid.NewGuid(),
                Priority = 1,
                Ident = 101,
                State = WorkQueueItemState.Completed,
                KeyValue = "test-key",
                Status = "Status",
                Tags = new[] { "tag1", "tag2" },
                AttemptNumber = 2,
                LoadedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(1), TimeSpan.Zero)),
                DeferredDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(2), TimeSpan.Zero)),
                LockedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(3), TimeSpan.Zero)),
                CompletedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(4), TimeSpan.Zero)),
                ExceptionedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(5), TimeSpan.Zero)),
                LastUpdated = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(6), TimeSpan.Zero)),
                WorkTimeInSeconds = 10,
                AttemptWorkTimeInSeconds = 20,
                ExceptionReason = "ExceptionReason",
                Resource = "ResourceName"
            };

            workQueueItemNoDataXml.ToModelNoDataXml().ShouldBeEquivalentTo(new WorkQueueItemNoDataXmlModel()
            {
                Id = workQueueItemNoDataXml.Id,
                Priority = workQueueItemNoDataXml.Priority,
                Ident = workQueueItemNoDataXml.Ident,
                State = (Models.WorkQueueItemState)workQueueItemNoDataXml.State,
                KeyValue = workQueueItemNoDataXml.KeyValue,
                Status = workQueueItemNoDataXml.Status,
                Tags = workQueueItemNoDataXml.Tags,
                AttemptNumber = workQueueItemNoDataXml.AttemptNumber,
                LoadedDate = workQueueItemNoDataXml.LoadedDate is Some<DateTimeOffset> loadedDate ? new DateTimeOffset(loadedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                DeferredDate = workQueueItemNoDataXml.DeferredDate is Some<DateTimeOffset> deferredDate ? new DateTimeOffset(deferredDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                LockedDate = workQueueItemNoDataXml.LockedDate is Some<DateTimeOffset> lockedDate ? new DateTimeOffset(lockedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                CompletedDate = workQueueItemNoDataXml.CompletedDate is Some<DateTimeOffset> completedDate ? new DateTimeOffset(completedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                ExceptionedDate = workQueueItemNoDataXml.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? new DateTimeOffset(exceptionedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                LastUpdated = workQueueItemNoDataXml.LastUpdated is Some<DateTimeOffset> lastUpdated ? new DateTimeOffset(lastUpdated.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                WorkTimeInSeconds = workQueueItemNoDataXml.WorkTimeInSeconds,
                AttemptWorkTimeInSeconds = workQueueItemNoDataXml.AttemptWorkTimeInSeconds,
                ExceptionReason = workQueueItemNoDataXml.ExceptionReason,
                Resource = workQueueItemNoDataXml.Resource
            });
        }

        [Test]
        public void ToModel_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var dictionary = new Dictionary<string, DataValue> { { "Test", new DataValue() } };
            var rows = new List<IReadOnlyDictionary<string, DataValue>> { dictionary };

            var workQueueItem = new WorkQueueItem()
            {
                Id = Guid.NewGuid(),
                Priority = 1,
                Ident = 101,
                State = WorkQueueItemState.Completed,
                KeyValue = "test-key",
                Status = "Status",
                Tags = new[] { "tag1", "tag2" },
                AttemptNumber = 2,
                LoadedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(1), TimeSpan.Zero)),
                DeferredDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(2), TimeSpan.Zero)),
                LockedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(3), TimeSpan.Zero)),
                CompletedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(4), TimeSpan.Zero)),
                ExceptionedDate = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(5), TimeSpan.Zero)),
                LastUpdated = OptionHelper.Some(new DateTimeOffset(DateTime.UtcNow.AddHours(6), TimeSpan.Zero)),
                WorkTimeInSeconds = 10,
                AttemptWorkTimeInSeconds = 20,
                ExceptionReason = "ExceptionReason",
                Resource = "ResourceName",
                Data = new DataCollection()
                {
                    Rows = rows
                }
            };

            var result = workQueueItem.ToModel();
            result.ShouldBeEquivalentTo(new WorkQueueItemModel()
            {
                Id = workQueueItem.Id,
                Priority = workQueueItem.Priority,
                Ident = workQueueItem.Ident,
                State = (Models.WorkQueueItemState)workQueueItem.State,
                KeyValue = workQueueItem.KeyValue,
                Status = workQueueItem.Status,
                Tags = workQueueItem.Tags,
                AttemptNumber = workQueueItem.AttemptNumber,
                LoadedDate = workQueueItem.LoadedDate is Some<DateTimeOffset> loadedDate ? new DateTimeOffset(loadedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                DeferredDate = workQueueItem.DeferredDate is Some<DateTimeOffset> deferredDate ? new DateTimeOffset(deferredDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                LockedDate = workQueueItem.LockedDate is Some<DateTimeOffset> lockedDate ? new DateTimeOffset(lockedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                CompletedDate = workQueueItem.CompletedDate is Some<DateTimeOffset> completedDate ? new DateTimeOffset(completedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                ExceptionedDate = workQueueItem.ExceptionedDate is Some<DateTimeOffset> exceptionedDate ? new DateTimeOffset(exceptionedDate.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                LastUpdated = workQueueItem.LastUpdated is Some<DateTimeOffset> lastUpdated ? new DateTimeOffset(lastUpdated.Value.DateTime, TimeSpan.Zero) : (DateTimeOffset?)null,
                WorkTimeInSeconds = workQueueItem.WorkTimeInSeconds,
                AttemptWorkTimeInSeconds = workQueueItem.AttemptWorkTimeInSeconds,
                ExceptionReason = workQueueItem.ExceptionReason,
                Resource = workQueueItem.Resource,
                Data = workQueueItem.Data.ToModel()
            });
        }
    }
}
