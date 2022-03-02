namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using AutomateAppCore;
    using Func;
    using Server.Domain.Models.Pagination;

    using static Func.OptionHelper;
    using Domain.PagingTokens;

    public static class ProcessSessionMapper
    {
        public static Session ToDomainObject(this clsProcessSession @this) =>
           new Session
           {
               SessionId = @this.SessionID,
               SessionNumber = @this.SessionNum,
               ProcessId = @this.ProcessID,
               ProcessName = @this.ProcessName,
               UserName = @this.UserName,
               ResourceId = @this.ResourceID,
               ResourceName = @this.ResourceName,
               Status = (SessionStatus)@this.Status,
               StartTime = @this.SessionStart == DateTimeOffset.MinValue ? None<DateTimeOffset>() : Some(@this.SessionStart),
               EndTime = @this.SessionEnd == DateTimeOffset.MaxValue ? None<DateTimeOffset>() : Some(@this.SessionEnd),
               StageStarted = @this.LastUpdated == DateTimeOffset.MinValue ? None<DateTimeOffset>() : Some(@this.LastUpdated),
               LatestStage = @this.LastStage,
               ExceptionMessage = @this.ExceptionMessage,
               ExceptionType = @this.ExceptionType == null ? None<string>() : Some(@this.ExceptionType),
               TerminationReason = (Domain.SessionTerminationReason)@this.SessionTerminationReason
           };

        public static ItemsPage<Session> ToItemsPage(this IReadOnlyList<Session> sessions, SessionParameters sessionsParameters) =>
            new ItemsPage<Session>
            {
                Items = sessions,
                PagingToken = GetPagingToken(sessions, sessionsParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyList<Session> items, SessionParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return None<string>();

            var lastItem = items.Last();
            var lastItemSortValue = GetPropertyValue(lastItem, parameters.SortBy);

            var pagingToken = new PagingToken<long>
            {
                PreviousIdValue = lastItem.SessionNumber,
                DataType = lastItemSortValue.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
                PreviousSortColumnValue = PaginationValueTypeFormatter.GetStringValueFromObject(lastItemSortValue)
            };
            return Some(pagingToken.ToString());
        }

        private static object GetPropertyValue(Session item, SessionSortByProperty index)
        {
            switch (index)
            {
                case SessionSortByProperty.ProcessNameAsc:
                case SessionSortByProperty.ProcessNameDesc:
                    return item.ProcessName;
                case SessionSortByProperty.SessionNumberAsc:
                case SessionSortByProperty.SessionNumberDesc:
                    return item.SessionNumber;
                case SessionSortByProperty.ResourceNameAsc:
                case SessionSortByProperty.ResourceNameDesc:
                    return item.ResourceName;
                case SessionSortByProperty.UserAsc:
                case SessionSortByProperty.UserDesc:
                    return item.UserName;
                case SessionSortByProperty.StatusAsc:
                case SessionSortByProperty.StatusDesc:
                    return item.Status;
                case SessionSortByProperty.StartTimeAsc:
                case SessionSortByProperty.StartTimeDesc:
                    return item.StartTime;
                case SessionSortByProperty.EndTimeAsc:
                case SessionSortByProperty.EndTimeDesc:
                    return item.EndTime;
                case SessionSortByProperty.StageStartedAsc:
                case SessionSortByProperty.StageStartedDesc:
                    return item.StageStarted;
                case SessionSortByProperty.LatestStageAsc:
                case SessionSortByProperty.LatestStageDesc:
                    return item.LatestStage;
                case SessionSortByProperty.ExceptionTypeAsc:
                case SessionSortByProperty.ExceptionTypeDesc:
                    return item.ExceptionType;
                default:
                    throw new ArgumentException($"Unknown {nameof(SessionSortByProperty)} index");
            }
        }

    }
}
