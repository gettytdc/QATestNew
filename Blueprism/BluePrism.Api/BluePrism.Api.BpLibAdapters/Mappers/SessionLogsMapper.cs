namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using AutomateAppCore;
    using Domain;
    using Domain.PagingTokens;
    using Func;
    using static Func.OptionHelper;

    public static class SessionLogsMapper
    {
        public static SessionLogItem ToDomainObject(this clsSessionLogEntry sessionLogEntry) =>
            new SessionLogItem
            {
                StageName = sessionLogEntry.StageName,
                StageType = (StageTypes)sessionLogEntry.StageType,
                ResourceStartTime = sessionLogEntry.StartDate == DateTimeOffset.MinValue ? None<DateTimeOffset>() : Some(sessionLogEntry.StartDate),
                Result = sessionLogEntry.Result,
                LogId = sessionLogEntry.LogId,
                HasParameters = !string.IsNullOrEmpty(sessionLogEntry.AttributeXml)
            };

        public static ItemsPage<SessionLogItem> ToItemsPage(this IReadOnlyList<SessionLogItem> sessionLogItems, SessionLogsParameters sessionLogsParameters) =>
            new ItemsPage<SessionLogItem>()
            {
                Items = sessionLogItems,
                PagingToken = GetPagingToken(sessionLogItems, sessionLogsParameters)
            };

        private static Option<string> GetPagingToken(IReadOnlyList<SessionLogItem> items, SessionLogsParameters parameters)
        {
            if (items.Count < parameters.ItemsPerPage)
                return None<string>();

            var lastItem = items[items.Count - 1];

            var pagingToken = new PagingToken<long>
            {
                PreviousIdValue = lastItem.LogId,
                DataType = lastItem.ResourceStartTime.GetTypeName(),
                ParametersHashCode = parameters.GetHashCodeForValidation(),
            };
            return Some(pagingToken.ToString());
        }
    }
}
