namespace BluePrism.Api.Mappers
{
    using System.Collections.Generic;
    using Domain;
    using Func;
    using Models;
    using static Func.OptionHelper;

    public static class WorkQueueSortByMapper
    {
        private static readonly IDictionary<WorkQueueSortBy, WorkQueueSortByProperty> WorkQueueSortByMappings =
            new Dictionary<WorkQueueSortBy, WorkQueueSortByProperty>
            {
                {Models.WorkQueueSortBy.NameAsc, Domain.WorkQueueSortByProperty.NameAsc},
                {Models.WorkQueueSortBy.NameDesc, Domain.WorkQueueSortByProperty.NameDesc},
                {Models.WorkQueueSortBy.StatusAsc, Domain.WorkQueueSortByProperty.RunningAsc},
                {Models.WorkQueueSortBy.StatusDesc, Domain.WorkQueueSortByProperty.RunningDesc},
                {Models.WorkQueueSortBy.KeyFieldAsc, Domain.WorkQueueSortByProperty.KeyFieldAsc},
                {Models.WorkQueueSortBy.KeyFieldDesc, Domain.WorkQueueSortByProperty.KeyFieldDesc},
                {Models.WorkQueueSortBy.MaxAttemptsAsc, Domain.WorkQueueSortByProperty.MaxAttemptsAsc},
                {Models.WorkQueueSortBy.MaxAttemptsDesc, Domain.WorkQueueSortByProperty.MaxAttemptsDesc},
                {Models.WorkQueueSortBy.IsEncryptedAsc, Domain.WorkQueueSortByProperty.EncryptIdAsc},
                {Models.WorkQueueSortBy.IsEncryptedDesc, Domain.WorkQueueSortByProperty.EncryptIdDesc},
                {Models.WorkQueueSortBy.TotalItemCountAsc, Domain.WorkQueueSortByProperty.TotalAsc},
                {Models.WorkQueueSortBy.TotalItemCountDesc, Domain.WorkQueueSortByProperty.TotalDesc},
                {Models.WorkQueueSortBy.CompletedItemCountAsc, Domain.WorkQueueSortByProperty.CompletedAsc},
                {Models.WorkQueueSortBy.CompletedItemCountDesc, Domain.WorkQueueSortByProperty.CompletedDesc},
                {Models.WorkQueueSortBy.LockedItemCountAsc, Domain.WorkQueueSortByProperty.LockedAsc},
                {Models.WorkQueueSortBy.LockedItemCountDesc, Domain.WorkQueueSortByProperty.LockedDesc},
                {Models.WorkQueueSortBy.PendingItemCountAsc, Domain.WorkQueueSortByProperty.PendingAsc},
                {Models.WorkQueueSortBy.PendingItemCountDesc, Domain.WorkQueueSortByProperty.PendingDesc},
                {Models.WorkQueueSortBy.ExceptionedItemCountAsc, Domain.WorkQueueSortByProperty.ExceptionedAsc},
                {Models.WorkQueueSortBy.ExceptionedItemCountDesc, Domain.WorkQueueSortByProperty.ExceptionedDesc},
                {Models.WorkQueueSortBy.TotalCaseDurationAsc, Domain.WorkQueueSortByProperty.TotalWorkTimeAsc},
                {Models.WorkQueueSortBy.TotalCaseDurationDesc, Domain.WorkQueueSortByProperty.TotalWorkTimeDesc},
                {Models.WorkQueueSortBy.AverageWorkTimeAsc, Domain.WorkQueueSortByProperty.AverageWorkedTimeAsc},
                {Models.WorkQueueSortBy.AverageWorkTimeDesc, Domain.WorkQueueSortByProperty.AverageWorkedTimeDesc},
            };

        public static Option<Domain.WorkQueueSortByProperty> GetWorkQueueSortByModelName(Models.WorkQueueSortBy? value)
        {
            var sortBy = value ?? WorkQueueSortBy.NameAsc;

            return WorkQueueSortByMappings.TryGetValue(sortBy, out var workQueueProperty)
                ? Some(workQueueProperty)
                : None<Domain.WorkQueueSortByProperty>();
        }
    }
}
