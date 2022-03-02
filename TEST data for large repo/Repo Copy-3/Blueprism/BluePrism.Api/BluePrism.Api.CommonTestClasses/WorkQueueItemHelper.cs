namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using BpLibAdapters.Mappers.FilterMappers;
    using Domain;
    using Domain.Filters;
    using FluentAssertions;

    public class WorkQueueItemHelper
    {
        public static void ValidateParametersModelsAreEqual(Server.Domain.Models.WorkQueueItemParameters bluePrismWorkQueueItemParameters, WorkQueueItemParameters domainWorkQueueItemsParameters)
        {
            bluePrismWorkQueueItemParameters.SortBy.ToString().Should().Be(domainWorkQueueItemsParameters.SortBy.ToString());
            bluePrismWorkQueueItemParameters.KeyValue.Should()
                .Be(domainWorkQueueItemsParameters.KeyValue.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.Status.Should()
                .Be(domainWorkQueueItemsParameters.Status.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.ExceptionReason.Should()
                .Be(domainWorkQueueItemsParameters.ExceptionReason.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.Attempt.Should()
                .Be(domainWorkQueueItemsParameters.Attempt.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.Priority.Should()
                .Be(domainWorkQueueItemsParameters.Priority.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.WorkTime.Should()
                .Be(domainWorkQueueItemsParameters.WorkTime.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.LoadedDate.Should()
                .Be(domainWorkQueueItemsParameters.LoadedDate.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.LockedDate.Should()
                .Be(domainWorkQueueItemsParameters.LockedDate.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.LastUpdated.Should()
                .Be(domainWorkQueueItemsParameters.LastUpdated.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.CompletedDate.Should()
                .Be(domainWorkQueueItemsParameters.CompletedDate.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.DeferredDate.Should()
                .Be(domainWorkQueueItemsParameters.DeferredDate.ToBluePrismObject());
            bluePrismWorkQueueItemParameters.ExceptionedDate.Should()
                .Be(domainWorkQueueItemsParameters.ExceptionedDate.ToBluePrismObject());


        }

        public static WorkQueueItemParameters GetTestDomainWorkQueueParameters() =>
            new WorkQueueItemParameters
            {
                SortBy = WorkQueueItemSortByProperty.LastUpdatedDesc,
                KeyValue = new StringStartsWithFilter("key"),
                Status = new StringStartsWithFilter("test"),
                ExceptionReason = new StringStartsWithFilter("ex"),
                Attempt = new GreaterThanOrEqualToFilter<int>(5),
                Priority = new EqualsFilter<int>(2),
                WorkTime = new EqualsFilter<int>(0),
                LoadedDate = new GreaterThanOrEqualToFilter<DateTimeOffset>(DateTime.UtcNow),
                LockedDate = new GreaterThanOrEqualToFilter<DateTimeOffset>(DateTime.UtcNow),
                LastUpdated = new EqualsFilter<DateTimeOffset>(DateTime.UtcNow),
                CompletedDate = new EqualsFilter<DateTimeOffset>(DateTime.UtcNow),
                DeferredDate = new LessThanOrEqualToFilter<DateTimeOffset>(DateTime.UtcNow),
                ExceptionedDate = new LessThanOrEqualToFilter<DateTimeOffset>(DateTime.UtcNow)
            };
    }
}
