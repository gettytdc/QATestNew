namespace BluePrism.Api.UnitTests.Mappers
{
    using System;
    using Api.Mappers;
    using Domain;
    using FluentAssertions;
    using Models;
    using NUnit.Framework;
    using QueueStatus = Models.QueueStatus;

    [TestFixture]
    public class WorkQueueMapperTests
    {

        [Test]
        public void ToModel_ShouldReturnCorrectlyMappedModel_WhenCalled()
        {
            var workQueue = new WorkQueue
            {
                Name = "testQueue1",
                Status = Domain.QueueStatus.Paused,
                IsEncrypted = true,
                KeyField = "testKey",
                MaxAttempts = 2,
                PendingItemCount = 1,
                CompletedItemCount = 1,
                ExceptionedItemCount = 0,
                LockedItemCount = 0,
                TotalItemCount = 2,
                AverageWorkTime = new TimeSpan(3, 2, 2),
                TotalCaseDuration = new TimeSpan(3, 2, 2)
            };

            workQueue.ToModel().ShouldBeEquivalentTo(new WorkQueueModel
            {
                Name = workQueue.Name,
                Status = (QueueStatus) workQueue.Status,
                KeyField = workQueue.KeyField,
                IsEncrypted = workQueue.IsEncrypted,
                MaxAttempts = workQueue.MaxAttempts,
                PendingItemCount = workQueue.PendingItemCount,
                CompletedItemCount = workQueue.CompletedItemCount,
                ExceptionedItemCount = workQueue.ExceptionedItemCount,
                LockedItemCount = workQueue.LockedItemCount,
                TotalItemCount = workQueue.TotalItemCount,
                AverageWorkTime = workQueue.AverageWorkTime,
                TotalCaseDuration = workQueue.TotalCaseDuration
            });
        }
    }
}
