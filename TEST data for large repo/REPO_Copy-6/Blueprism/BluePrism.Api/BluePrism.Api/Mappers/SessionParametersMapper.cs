namespace BluePrism.Api.Mappers
{
    using System;
    using System.Linq;
    using Domain.Filters;
    using FilterMappers;
    using Func;
    using Models;

    using static SessionSortByMapper;

    public static class SessionParametersMapper
    {
        private const int DefaultItemsPerPage = 10;

        public static Domain.SessionParameters ToDomainObject(this Models.SessionParameters sessionParameters) =>
            new Domain.SessionParameters
            {
                SortBy = sessionParameters.SortBy.ToDomainSortBy(),
                ItemsPerPage = sessionParameters.ItemsPerPage ?? DefaultItemsPerPage,
                PagingToken = sessionParameters.PagingToken.ToDomainPagingToken(),
                ProcessName = sessionParameters.ProcessName.ToDomain(),
                SessionNumber = sessionParameters.SessionNumber.ToDomain(),
                ResourceName = sessionParameters.ResourceName.ToDomain(),
                User = sessionParameters.UserName.ToDomain(),
                Status = sessionParameters.Status?.Select(x => x.ToDomain()).ToMultiValueFilter()
                         ?? new MultiValueFilter<Domain.SessionStatus>(new Filter<Domain.SessionStatus>[0]),
                StartTime = sessionParameters.StartTime.ToDomain(x => x ?? DateTimeOffset.MinValue),
                EndTime = sessionParameters.EndTime.ToDomain(x => x ?? DateTimeOffset.MaxValue),
                LatestStage = sessionParameters.LatestStage.ToDomain(),
                StageStarted = sessionParameters.StageStarted.ToDomain(x => x ?? DateTimeOffset.MinValue)
            };

        private static Domain.SessionStatus ToDomain(this Models.SessionStatus sessionStatus)
        {
            switch (sessionStatus)
            {
                case SessionStatus.Running:
                    return Domain.SessionStatus.Running;
                case SessionStatus.Terminated:
                    return Domain.SessionStatus.Terminated;
                case SessionStatus.Completed:
                    return Domain.SessionStatus.Completed;
                case SessionStatus.Stopped:
                    return Domain.SessionStatus.Stopped;
                case SessionStatus.Stopping:
                    return Domain.SessionStatus.Stopping;
                case SessionStatus.Warning:
                    return Domain.SessionStatus.Warning;
                case SessionStatus.Pending:
                    return Domain.SessionStatus.Pending;
                default:
                    throw new ArgumentException("Unexpected session status", nameof(SessionStatus));
            }
        }

        private static Domain.SessionSortByProperty ToDomainSortBy(this SessionSortBy? @this)
        {
            switch (GetProcessSessionSortByModelName(@this))
            {
                case Some<Domain.SessionSortByProperty> x:
                    return x.Value;
                default:
                    throw new ArgumentException("Unexpected sort by", nameof(Models.SessionParameters.SortBy));
            }
        }
    }
}
